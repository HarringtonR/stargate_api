using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using StargateAPI.Controllers;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Dtos;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;

namespace StargateAPI.Tests;

public class PersonControllerTests
{
    [Fact]
    public async Task GetPeople_ReturnsObjectResult_WhenSuccessful()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new GetPeopleResult 
        { 
            Success = true,
            People = new List<PersonAstronaut>
            {
                new PersonAstronaut { PersonId = 1, Name = "John Doe", CurrentRank = "Captain", CurrentDutyTitle = "Commander" },
                new PersonAstronaut { PersonId = 2, Name = "Jane Smith", CurrentRank = "Major", CurrentDutyTitle = "Pilot" }
            }
        };

        mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPeople();

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify the mediator was called
        mediator.Verify(m => m.Send(It.IsAny<GetPeople>(), default), Times.Once);
    }

    [Fact]
    public async Task GetPeople_ReturnsObjectResult_WhenNoPeopleFound()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new GetPeopleResult 
        { 
            Success = true,
            People = new List<PersonAstronaut>()
        };

        mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPeople();

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task GetPeople_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
            .ThrowsAsync(new System.Exception("Database connection failed"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPeople();

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
    public async Task GetPeople_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPeople();

        // Assert - This covers the catch branch!
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Database connection failed", response.Message);
        Assert.Equal(500, response.ResponseCode);
    }

    [Fact]
    public async Task GetPersonByName_ReturnsObjectResult_WhenPersonExists()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new GetPersonByNameResult 
        { 
            Success = true,
            Person = new PersonAstronaut { PersonId = 1, Name = "John Doe", CurrentRank = "Captain", CurrentDutyTitle = "Commander" }
        };

        mediator.Setup(m => m.Send(It.Is<GetPersonByName>(q => q.Name == "John Doe"), default))
            .ReturnsAsync(expectedResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPersonByName("John Doe");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify the mediator was called with correct parameters
        mediator.Verify(m => m.Send(It.Is<GetPersonByName>(q => q.Name == "John Doe"), default), Times.Once);
    }

    [Fact]
    public async Task GetPersonByName_ReturnsObjectResult_WhenPersonNotFound()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new GetPersonByNameResult 
        { 
            Success = true,
            Person = null
        };

        mediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPersonByName("NonExistent Person");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task GetPersonByName_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), default))
            .ThrowsAsync(new System.Exception("Database connection failed"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPersonByName("John Doe");

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
    public async Task GetPersonByName_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), default))
            .ThrowsAsync(new TimeoutException("Request timeout"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPersonByName("John Doe");

        // Assert - This covers the catch branch!
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Request timeout", response.Message);
        Assert.Equal(500, response.ResponseCode);
    }

    [Fact]
    public async Task CreatePerson_ReturnsObjectResult_WhenPersonCreated()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new CreatePersonResult 
        { 
            Success = true, 
            Id = 123,
            Message = "Person created successfully",
            ResponseCode = 200
        };

        mediator.Setup(m => m.Send(It.Is<CreatePerson>(cmd => cmd.Name == "New Name"), default))
            .ReturnsAsync(expectedResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.CreatePerson("New Name");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify the mediator was called with correct parameters
        mediator.Verify(m => m.Send(It.Is<CreatePerson>(cmd => cmd.Name == "New Name"), default), Times.Once);
    }

    [Fact]
    public async Task CreatePerson_ReturnsInternalServerError_WhenPersonAlreadyExists()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreatePerson>(), default))
            .ThrowsAsync(new BadHttpRequestException("Bad Request"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.CreatePerson("Existing Name");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Bad Request", response.Message);
    }

    [Fact]
    public async Task CreatePerson_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreatePerson>(), default))
            .ThrowsAsync(new System.Exception("Database connection failed"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.CreatePerson("New Name");

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
    public async Task CreatePerson_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreatePerson>(), default))
            .ThrowsAsync(new ArgumentException("Invalid person name"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.CreatePerson("Test Person");

        // Assert - This covers the catch branch!
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Invalid person name", response.Message);
        Assert.Equal(500, response.ResponseCode);
    }

    [Fact]
    public async Task UpdatePerson_ReturnsObjectResult_WhenPersonUpdated()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new UpdatePersonResult 
        { 
            Success = true, 
            Id = 123,
            Message = "Person updated successfully",
            ResponseCode = 200
        };

        mediator.Setup(m => m.Send(It.Is<UpdatePerson>(cmd => cmd.CurrentName == "Old Name" && cmd.NewName == "New Name"), default))
            .ReturnsAsync(expectedResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.UpdatePerson("Old Name", "New Name");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        // Verify the mediator was called with correct parameters
        mediator.Verify(m => m.Send(It.Is<UpdatePerson>(cmd => 
            cmd.CurrentName == "Old Name" && cmd.NewName == "New Name"), default), Times.Once);
    }

    [Fact]
    public async Task UpdatePerson_ReturnsInternalServerError_WhenPersonNotFound()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<UpdatePerson>(), default))
            .ThrowsAsync(new BadHttpRequestException("Person with name 'NonExistent' not found"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.UpdatePerson("NonExistent", "New Name");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task UpdatePerson_ReturnsInternalServerError_WhenNewNameAlreadyExists()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<UpdatePerson>(), default))
            .ThrowsAsync(new BadHttpRequestException("Person with name 'Existing Name' already exists"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.UpdatePerson("Old Name", "Existing Name");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Contains("already exists", response.Message);
    }

    [Fact]
    public async Task UpdatePerson_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<UpdatePerson>(), default))
            .ThrowsAsync(new System.Exception("Database connection failed"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.UpdatePerson("Old Name", "New Name");

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
    public async Task UpdatePerson_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<UpdatePerson>(), default))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.UpdatePerson("Old Name", "New Name");

        // Assert - This covers the catch branch!
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Access denied", response.Message);
        Assert.Equal(500, response.ResponseCode);
    }

    [Fact]
    public async Task UpdatePerson_HandlesEmptyCurrentName()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<UpdatePerson>(), default))
            .ThrowsAsync(new BadHttpRequestException("Person with name '' not found"));

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.UpdatePerson("", "New Name");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
    }

    [Fact]
    public async Task UpdatePerson_HandlesEmptyNewName()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new UpdatePersonResult 
        { 
            Success = true, 
            Id = 123,
            Message = "Person updated successfully",
            ResponseCode = 200
        };

        mediator.Setup(m => m.Send(It.Is<UpdatePerson>(cmd => cmd.CurrentName == "Old Name" && cmd.NewName == ""), default))
            .ReturnsAsync(expectedResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.UpdatePerson("Old Name", "");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task UpdatePerson_HandlesSameCurrentAndNewName()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var expectedResult = new UpdatePersonResult 
        { 
            Success = true, 
            Id = 123,
            Message = "Person updated successfully",
            ResponseCode = 200
        };

        mediator.Setup(m => m.Send(It.Is<UpdatePerson>(cmd => cmd.CurrentName == "Same Name" && cmd.NewName == "Same Name"), default))
            .ReturnsAsync(expectedResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.UpdatePerson("Same Name", "Same Name");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }
}