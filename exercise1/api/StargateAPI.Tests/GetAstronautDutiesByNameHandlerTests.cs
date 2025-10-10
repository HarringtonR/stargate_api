using Xunit;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Data;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace StargateAPI.Tests;

public class GetAstronautDutiesByNameHandlerTests
{
    private StargateContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<StargateContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new StargateContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsErrorResponse_WhenNameIsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetAstronautDutiesByNameHandler(context);
        var request = new GetAstronautDutiesByName { Name = null! };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Name cannot be empty or null", result.Message);
        Assert.Equal(400, result.ResponseCode);
        Assert.Null(result.Person);
        Assert.Empty(result.AstronautDuties);
    }

    [Fact]
    public async Task Handle_ReturnsErrorResponse_WhenNameIsEmpty()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetAstronautDutiesByNameHandler(context);
        var request = new GetAstronautDutiesByName { Name = "" };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Name cannot be empty or null", result.Message);
        Assert.Equal(400, result.ResponseCode);
        Assert.Null(result.Person);
        Assert.Empty(result.AstronautDuties);
    }

    [Fact]
    public async Task Handle_ReturnsErrorResponse_WhenNameIsWhitespace()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetAstronautDutiesByNameHandler(context);
        var request = new GetAstronautDutiesByName { Name = "   " };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Name cannot be empty or null", result.Message);
        Assert.Equal(400, result.ResponseCode);
        Assert.Null(result.Person);
        Assert.Empty(result.AstronautDuties);
    }

    [Fact]
    public async Task Handle_ValidatesMultpleInputFormats_ImprovingBranchCoverage()
    {
        // This test specifically targets the input validation branches
        // to improve branch coverage metrics
        
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetAstronautDutiesByNameHandler(context);

        // Test various invalid input formats
        var testCases = new[]
        {
            ("", "Empty string"),
            (null!, "Null value"),
            ("   ", "Whitespace only"),
            ("\t\n\r", "Tab and newlines"),
            ("  \t  ", "Mixed whitespace")
        };

        foreach (var (name, description) in testCases)
        {
            // Act
            var request = new GetAstronautDutiesByName { Name = name };
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert - Fixed parameter order: boolean first, message second
            Assert.False(result.Success, $"Should fail for {description}");
            Assert.Equal(400, result.ResponseCode);
            Assert.Equal("Name cannot be empty or null", result.Message);
            Assert.Null(result.Person);
            Assert.Empty(result.AstronautDuties);
        }
    }
}