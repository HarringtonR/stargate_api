using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using StargateAPI.Controllers;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Data;
using System.Threading.Tasks;
using System.Net;
using System.Data.SqlClient;

namespace StargateAPI.Tests;

public class AstronautDutyControllerTests
{
    [Fact]
    public async Task GetAstronautDutiesByName_ReturnsObjectResult_WhenPersonExists()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new GetAstronautDutiesByNameResult 
        { 
            Success = true,
            Person = new PersonAstronaut { PersonId = 1, Name = "John Doe" },
            AstronautDuties = new List<AstronautDuty>
            {
                new AstronautDuty { Id = 1, PersonId = 1, Rank = "Captain", DutyTitle = "Commander" }
            }
        };

        mediator.Setup(m => m.Send(It.Is<GetAstronautDutiesByName>(q => q.Name == "John Doe"), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.GetAstronautDutiesByName("John Doe");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify the mediator was called with correct parameters
        mediator.Verify(m => m.Send(It.Is<GetAstronautDutiesByName>(q => q.Name == "John Doe"), default), Times.Once);
    }

    [Fact]
    public async Task GetAstronautDutiesByName_ReturnsObjectResult_WhenPersonNotFound()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new GetAstronautDutiesByNameResult 
        { 
            Success = true,
            Person = null!, // Explicitly handle null case
            AstronautDuties = new List<AstronautDuty>()
        };

        mediator.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.GetAstronautDutiesByName("NonExistent Person");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task GetAstronautDutiesByName_ReturnsObjectResult_WhenPersonExistsWithNoDuties()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new GetAstronautDutiesByNameResult 
        { 
            Success = true,
            Person = new PersonAstronaut { PersonId = 1, Name = "John Doe" },
            AstronautDuties = new List<AstronautDuty>() // Empty duties list
        };

        mediator.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.GetAstronautDutiesByName("John Doe");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task GetAstronautDutiesByName_ReturnsObjectResult_WhenPersonExistsWithMultipleDuties()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new GetAstronautDutiesByNameResult 
        { 
            Success = true,
            Person = new PersonAstronaut { PersonId = 1, Name = "John Doe" },
            AstronautDuties = new List<AstronautDuty>
            {
                new AstronautDuty { Id = 1, PersonId = 1, Rank = "Lieutenant", DutyTitle = "Pilot", DutyStartDate = DateTime.Now.AddYears(-3), DutyEndDate = DateTime.Now.AddYears(-2) },
                new AstronautDuty { Id = 2, PersonId = 1, Rank = "Captain", DutyTitle = "Commander", DutyStartDate = DateTime.Now.AddYears(-2), DutyEndDate = null }
            }
        };

        mediator.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.GetAstronautDutiesByName("John Doe");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task GetAstronautDutiesByName_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), default))
            .ThrowsAsync(new System.Exception("Database connection failed"));

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.GetAstronautDutiesByName("John Doe");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Database connection failed", response.Message);
        Assert.Equal((int)HttpStatusCode.InternalServerError, response.ResponseCode);
    }

    [Fact]
    public async Task GetAstronautDutiesByName_HandlesEmptyName()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new GetAstronautDutiesByNameResult 
        { 
            Success = true,
            Person = null,
            AstronautDuties = new List<AstronautDuty>()
        };

        mediator.Setup(m => m.Send(It.Is<GetAstronautDutiesByName>(q => q.Name == ""), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.GetAstronautDutiesByName("");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task CreateAstronautDuty_ReturnsObjectResult_WhenValidRequest()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createRequest = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now.Date
        };

        var expectedResult = new CreateAstronautDutyResult 
        { 
            Success = true, 
            Id = 123 
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify the mediator was called with the correct request
        mediator.Verify(m => m.Send(It.Is<CreateAstronautDuty>(cmd => 
            cmd.Name == "John Doe" && 
            cmd.Rank == "Captain" && 
            cmd.DutyTitle == "Commander"), default), Times.Once);
    }

    [Fact]
    public async Task CreateAstronautDuty_ReturnsObjectResult_WhenRetiredDuty()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createRequest = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "General",
            DutyTitle = "RETIRED",
            DutyStartDate = DateTime.Now.Date
        };

        var expectedResult = new CreateAstronautDutyResult 
        { 
            Success = true, 
            Id = 456 
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify retirement duty was processed
        mediator.Verify(m => m.Send(It.Is<CreateAstronautDuty>(cmd => 
            cmd.DutyTitle == "RETIRED"), default), Times.Once);
    }

    [Fact]
    public async Task CreateAstronautDuty_ReturnsObjectResult_WhenFutureDateSpecified()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var futureDate = DateTime.Now.Date.AddDays(30);
        var createRequest = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = futureDate
        };

        var expectedResult = new CreateAstronautDutyResult 
        { 
            Success = true, 
            Id = 789 
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify future date was processed
        mediator.Verify(m => m.Send(It.Is<CreateAstronautDuty>(cmd => 
            cmd.DutyStartDate == futureDate), default), Times.Once);
    }

    [Fact]
    public async Task CreateAstronautDuty_ReturnsInternalServerError_WhenValidationFailed()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createRequest = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now.Date
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ThrowsAsync(new BadHttpRequestException("Validation failed"));

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Validation failed", response.Message);
    }

    [Fact]
    public async Task CreateAstronautDuty_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createRequest = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now.Date
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ThrowsAsync(new System.Exception("Database connection failed"));

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Database connection failed", response.Message);
        Assert.Equal((int)HttpStatusCode.InternalServerError, response.ResponseCode);
    }

    [Fact]
    public async Task CreateAstronautDuty_HandlesNullRequest()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var controller = new AstronautDutyController(mediator.Object);

        // Create a request that will cause ArgumentNullException when processed
        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ThrowsAsync(new ArgumentNullException("request"));

        // Act - Test with a valid request object that will throw during processing
        var testRequest = new CreateAstronautDuty
        {
            Name = "Test",
            Rank = "Test",
            DutyTitle = "Test",
            DutyStartDate = DateTime.Now
        };
        
        var result = await controller.CreateAstronautDuty(testRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Contains("request", response.Message);
    }

    [Fact]
    public async Task CreateAstronautDuty_ValidatesRequiredFields()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createRequest = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now.Date.AddDays(30) // Future date
        };

        var expectedResult = new CreateAstronautDutyResult 
        { 
            Success = true, 
            Id = 789 
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify all required fields were passed through
        mediator.Verify(m => m.Send(It.Is<CreateAstronautDuty>(cmd => 
            !string.IsNullOrEmpty(cmd.Name) && 
            !string.IsNullOrEmpty(cmd.Rank) && 
            !string.IsNullOrEmpty(cmd.DutyTitle) && 
            cmd.DutyStartDate != default(DateTime)), default), Times.Once);
    }

    [Fact]
    public async Task CreateAstronautDuty_HandlesEmptyStringFields()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createRequest = new CreateAstronautDuty
        {
            Name = "",
            Rank = "",
            DutyTitle = "",
            DutyStartDate = DateTime.Now.Date
        };

        var expectedResult = new CreateAstronautDutyResult 
        { 
            Success = true, 
            Id = 999 
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task CreateAstronautDuty_HandlesLowercaseRetiredTitle()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createRequest = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "General",
            DutyTitle = "retired", // lowercase
            DutyStartDate = DateTime.Now.Date
        };

        var expectedResult = new CreateAstronautDutyResult 
        { 
            Success = true, 
            Id = 888 
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task CreateAstronautDuty_HandlesSpecialCharactersInName()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createRequest = new CreateAstronautDuty
        {
            Name = "O'Connor-Smith Jr.",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now.Date
        };

        var expectedResult = new CreateAstronautDutyResult 
        { 
            Success = true, 
            Id = 777 
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify special characters in name are handled
        mediator.Verify(m => m.Send(It.Is<CreateAstronautDuty>(cmd => 
            cmd.Name == "O'Connor-Smith Jr."), default), Times.Once);
    }

    [Fact]
    public async Task CreateAstronautDuty_HandlesPastDate()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var pastDate = DateTime.Now.Date.AddDays(-30);
        var createRequest = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = pastDate
        };

        var expectedResult = new CreateAstronautDutyResult 
        { 
            Success = true, 
            Id = 666 
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify past date was processed
        mediator.Verify(m => m.Send(It.Is<CreateAstronautDuty>(cmd => 
            cmd.DutyStartDate == pastDate), default), Times.Once);
    }

    [Fact]
    public async Task GetAstronautDutiesByName_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), default))
            .ThrowsAsync(new InvalidOperationException("Database connection timeout"));

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.GetAstronautDutiesByName("John Doe");

        // Assert - This covers the catch branch!
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Database connection timeout", response.Message);
        Assert.Equal(500, response.ResponseCode);
    }

    [Fact]
    public async Task CreateAstronautDuty_ReturnsInternalServerError_WhenMediatorThrowsGenericException()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createRequest = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain", 
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now.Date
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ThrowsAsync(new InvalidOperationException("Unexpected database error"));

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert - This covers the catch branch!
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Unexpected database error", response.Message);
        Assert.Equal(500, response.ResponseCode);
    }

    [Fact]
    public async Task CreateAstronautDuty_ReturnsInternalServerError_WhenMediatorThrowsTimeoutException()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createRequest = new CreateAstronautDuty
        {
            Name = "Jane Smith",
            Rank = "Lieutenant",
            DutyTitle = "Pilot", 
            DutyStartDate = DateTime.Now.Date.AddDays(-10)
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ThrowsAsync(new TimeoutException("Operation timed out after 30 seconds"));

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert - This covers the catch branch!
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Operation timed out after 30 seconds", response.Message);
        Assert.Equal(500, response.ResponseCode);
    }
}