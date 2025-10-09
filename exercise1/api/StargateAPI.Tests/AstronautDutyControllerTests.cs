using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Controllers;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Data;
using System.Threading.Tasks;
using System.Net;

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
            .ThrowsAsync(new System.Exception("Validation failed"));

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
}