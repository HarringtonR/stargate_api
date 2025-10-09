using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Services;
using StargateAPI.Business.Data;
using System.Threading.Tasks;
using System;

namespace StargateAPI.Tests;

public class ProcessLoggingServiceTests
{
    private StargateContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<StargateContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new StargateContext(options);
    }

    [Fact]
    public async Task LogSuccessAsync_CreatesSuccessLogWithAllParameters()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);

        // Act
        await service.LogSuccessAsync("Test success message", "PersonController", "GetPeople", "test request data");

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("SUCCESS", log.Level);
        Assert.Equal("Test success message", log.Message);
        Assert.Equal("PersonController", log.Controller);
        Assert.Equal("GetPeople", log.Action);
        Assert.Equal("test request data", log.RequestData);
        Assert.Null(log.Exception);
        Assert.True(log.Timestamp <= DateTime.UtcNow);
        Assert.True(log.Timestamp >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task LogSuccessAsync_CreatesSuccessLogWithMinimalParameters()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);

        // Act
        await service.LogSuccessAsync("Test success message");

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("SUCCESS", log.Level);
        Assert.Equal("Test success message", log.Message);
        Assert.Null(log.Controller);
        Assert.Null(log.Action);
        Assert.Null(log.RequestData);
        Assert.Null(log.Exception);
    }

    [Fact]
    public async Task LogErrorAsync_CreatesErrorLogWithException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);
        var exception = new InvalidOperationException("Test exception");

        // Act
        await service.LogErrorAsync("Test error message", exception, "PersonController", "CreatePerson", "test request data");

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("ERROR", log.Level);
        Assert.Equal("Test error message", log.Message);
        Assert.Equal("PersonController", log.Controller);
        Assert.Equal("CreatePerson", log.Action);
        Assert.Equal("test request data", log.RequestData);
        Assert.Contains("Test exception", log.Exception);
        Assert.Contains("InvalidOperationException", log.Exception);
    }

    [Fact]
    public async Task LogErrorAsync_CreatesErrorLogWithoutException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);

        // Act
        await service.LogErrorAsync("Test error message without exception");

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("ERROR", log.Level);
        Assert.Equal("Test error message without exception", log.Message);
        Assert.Null(log.Exception);
        Assert.Null(log.Controller);
        Assert.Null(log.Action);
        Assert.Null(log.RequestData);
    }

    [Fact]
    public async Task LogInfoAsync_CreatesInfoLogWithAllParameters()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);

        // Act
        await service.LogInfoAsync("Test info message", "AstronautDutyController", "GetAstronautDutiesByName", "test request data");

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("INFO", log.Level);
        Assert.Equal("Test info message", log.Message);
        Assert.Equal("AstronautDutyController", log.Controller);
        Assert.Equal("GetAstronautDutiesByName", log.Action);
        Assert.Equal("test request data", log.RequestData);
        Assert.Null(log.Exception);
    }

    [Fact]
    public async Task LogInfoAsync_CreatesInfoLogWithMinimalParameters()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);

        // Act
        await service.LogInfoAsync("Test info message");

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("INFO", log.Level);
        Assert.Equal("Test info message", log.Message);
        Assert.Null(log.Controller);
        Assert.Null(log.Action);
        Assert.Null(log.RequestData);
        Assert.Null(log.Exception);
    }

    [Fact]
    public async Task LogWarningAsync_CreatesWarningLogWithAllParameters()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);

        // Act
        await service.LogWarningAsync("Test warning message", "PersonController", "UpdatePerson", "test request data");

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("WARNING", log.Level);
        Assert.Equal("Test warning message", log.Message);
        Assert.Equal("PersonController", log.Controller);
        Assert.Equal("UpdatePerson", log.Action);
        Assert.Equal("test request data", log.RequestData);
        Assert.Null(log.Exception);
    }

    [Fact]
    public async Task LogWarningAsync_CreatesWarningLogWithMinimalParameters()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);

        // Act
        await service.LogWarningAsync("Test warning message");

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("WARNING", log.Level);
        Assert.Equal("Test warning message", log.Message);
        Assert.Null(log.Controller);
        Assert.Null(log.Action);
        Assert.Null(log.RequestData);
        Assert.Null(log.Exception);
    }

    [Fact]
    public async Task LogAsync_HandlesLongMessage()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);
        var longMessage = new string('x', 5000); // Very long message

        // Act
        await service.LogSuccessAsync(longMessage);

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("SUCCESS", log.Level);
        Assert.Equal(longMessage, log.Message);
    }

    [Fact]
    public async Task LogAsync_HandlesSpecialCharacters()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);
        var messageWithSpecialChars = "Test message with special chars: @#$%^&*()[]{}|\\;':\"<>?,./ \t\n\r";

        // Act
        await service.LogInfoAsync(messageWithSpecialChars, "TestController", "TestAction", "Special data: <>{}[]");

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("INFO", log.Level);
        Assert.Equal(messageWithSpecialChars, log.Message);
        Assert.Equal("TestController", log.Controller);
        Assert.Equal("TestAction", log.Action);
        Assert.Equal("Special data: <>{}[]", log.RequestData);
    }

    [Fact]
    public async Task LogAsync_HandlesEmptyStringParameters()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);

        // Act
        await service.LogErrorAsync("", null, "", "", "");

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("ERROR", log.Level);
        Assert.Equal("", log.Message);
        Assert.Equal("", log.Controller);
        Assert.Equal("", log.Action);
        Assert.Equal("", log.RequestData);
        Assert.Null(log.Exception);
    }

    [Fact]
    public async Task LogAsync_CreatesMultipleLogs()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);

        // Act
        await service.LogSuccessAsync("First message");
        await service.LogErrorAsync("Second message", new Exception("Test"));
        await service.LogInfoAsync("Third message");
        await service.LogWarningAsync("Fourth message");

        // Assert
        var logs = await context.ProcessLogs.OrderBy(l => l.Id).ToListAsync();
        Assert.Equal(4, logs.Count);
        
        Assert.Equal("SUCCESS", logs[0].Level);
        Assert.Equal("First message", logs[0].Message);
        
        Assert.Equal("ERROR", logs[1].Level);
        Assert.Equal("Second message", logs[1].Message);
        Assert.NotNull(logs[1].Exception);
        
        Assert.Equal("INFO", logs[2].Level);
        Assert.Equal("Third message", logs[2].Message);
        
        Assert.Equal("WARNING", logs[3].Level);
        Assert.Equal("Fourth message", logs[3].Message);
    }

    [Fact]
    public async Task LogAsync_HandlesNestedExceptions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new ProcessLoggingService(context);
        var innerException = new ArgumentNullException("innerParam", "Inner exception message");
        var outerException = new InvalidOperationException("Outer exception message", innerException);

        // Act
        await service.LogErrorAsync("Test nested exception", outerException);

        // Assert
        var log = await context.ProcessLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("ERROR", log.Level);
        Assert.Equal("Test nested exception", log.Message);
        Assert.Contains("InvalidOperationException", log.Exception);
        Assert.Contains("Outer exception message", log.Exception);
        Assert.Contains("ArgumentNullException", log.Exception);
        Assert.Contains("Inner exception message", log.Exception);
    }

    [Fact]
    public async Task ProcessLoggingService_HandlesDatabaseSaveFailure()
    {
        // This test ensures that database save failures don't crash the application
        // but we can't easily test the console output without additional infrastructure
        // The service has a try-catch to handle this gracefully
        
        // Arrange
        using var context = CreateInMemoryContext();
        // Dispose the context to force it into an invalid state
        context.Dispose();
        var service = new ProcessLoggingService(context);

        // Act & Assert - Should not throw exception even if database operation fails
        await service.LogSuccessAsync("This should not crash the app");
    }
}