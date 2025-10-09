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
using System.Collections.Generic;

namespace StargateAPI.Tests;

public class ControllerExtensionTests
{
    [Fact]
    public async Task PersonController_GetResponse_ReturnsCorrectObjectResult_ForSuccessfulResult()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var successResult = new GetPeopleResult
        {
            Success = true,
            Message = "Operation successful",
            ResponseCode = 200,
            People = new List<PersonAstronaut>
            {
                new PersonAstronaut { PersonId = 1, Name = "Test Person" }
            }
        };

        mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
            .ReturnsAsync(successResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPeople();

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        var response = objectResult?.Value as GetPeopleResult;
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("Operation successful", response.Message);
        Assert.Equal(200, response.ResponseCode);
        Assert.Single(response.People);
    }

    [Fact]
    public async Task PersonController_GetResponse_ReturnsCorrectObjectResult_ForFailedResult()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var failedResult = new GetPersonByNameResult
        {
            Success = false,
            Message = "Person not found",
            ResponseCode = 404,
            Person = null
        };

        mediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), default))
            .ReturnsAsync(failedResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPersonByName("NonExistent");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(404, objectResult?.StatusCode);
        
        var response = objectResult?.Value as GetPersonByNameResult;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Person not found", response.Message);
        Assert.Equal(404, response.ResponseCode);
        Assert.Null(response.Person);
    }

    [Fact]
    public async Task AstronautDutyController_GetResponse_ReturnsCorrectObjectResult_ForSuccessfulResult()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var successResult = new GetAstronautDutiesByNameResult
        {
            Success = true,
            Message = "Retrieved duties successfully",
            ResponseCode = 200,
            Person = new PersonAstronaut { PersonId = 1, Name = "Test Person" },
            AstronautDuties = new List<AstronautDuty>
            {
                new AstronautDuty { Id = 1, PersonId = 1, Rank = "Captain", DutyTitle = "Commander" }
            }
        };

        mediator.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), default))
            .ReturnsAsync(successResult);

        var controller = new AstronautDutyController(mediator.Object);

        // Act
        var result = await controller.GetAstronautDutiesByName("Test Person");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        var response = objectResult?.Value as GetAstronautDutiesByNameResult;
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("Retrieved duties successfully", response.Message);
        Assert.Equal(200, response.ResponseCode);
        Assert.NotNull(response.Person);
        Assert.Single(response.AstronautDuties);
    }

    [Fact]
    public async Task AstronautDutyController_GetResponse_HandlesCreateResult()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createResult = new CreateAstronautDutyResult
        {
            Success = true,
            Message = "Duty created successfully",
            ResponseCode = 201,
            Id = 42
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(createResult);

        var controller = new AstronautDutyController(mediator.Object);
        var createRequest = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now.Date
        };

        // Act
        var result = await controller.CreateAstronautDuty(createRequest);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(201, objectResult?.StatusCode);
        
        var response = objectResult?.Value as CreateAstronautDutyResult;
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("Duty created successfully", response.Message);
        Assert.Equal(201, response.ResponseCode);
        Assert.Equal(42, response.Id);
    }

    [Fact]
    public async Task PersonController_GetResponse_HandlesCreatePersonResult()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var createResult = new CreatePersonResult
        {
            Success = true,
            Message = "Person created successfully",
            ResponseCode = 201,
            Id = 123
        };

        mediator.Setup(m => m.Send(It.IsAny<CreatePerson>(), default))
            .ReturnsAsync(createResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.CreatePerson("New Person");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(201, objectResult?.StatusCode);
        
        var response = objectResult?.Value as CreatePersonResult;
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("Person created successfully", response.Message);
        Assert.Equal(201, response.ResponseCode);
        Assert.Equal(123, response.Id);
    }

    [Fact]
    public async Task PersonController_GetResponse_HandlesUpdatePersonResult()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var updateResult = new UpdatePersonResult
        {
            Success = true,
            Message = "Person updated successfully",
            ResponseCode = 200,
            Id = 456
        };

        mediator.Setup(m => m.Send(It.IsAny<UpdatePerson>(), default))
            .ReturnsAsync(updateResult);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.UpdatePerson("Old Name", "New Name");

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
        
        var response = objectResult?.Value as UpdatePersonResult;
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("Person updated successfully", response.Message);
        Assert.Equal(200, response.ResponseCode);
        Assert.Equal(456, response.Id);
    }

    [Fact]
    public async Task Controllers_GetResponse_HandlesBaseResponseWithDifferentStatusCodes()
    {
        // Test various HTTP status codes through the extension method
        var testCases = new[]
        {
            (HttpStatusCode.OK, 200),
            (HttpStatusCode.Created, 201),
            (HttpStatusCode.BadRequest, 400),
            (HttpStatusCode.NotFound, 404),
            (HttpStatusCode.InternalServerError, 500)
        };

        foreach (var (statusCode, expectedCode) in testCases)
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var testResult = new GetPeopleResult
            {
                Success = statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.Created,
                Message = $"Test message for {statusCode}",
                ResponseCode = (int)statusCode,
                People = new List<PersonAstronaut>()
            };

            mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
                .ReturnsAsync(testResult);

            var controller = new PersonController(mediator.Object);

            // Act
            var result = await controller.GetPeople();

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(expectedCode, objectResult?.StatusCode);
            
            var response = objectResult?.Value as GetPeopleResult;
            Assert.NotNull(response);
            Assert.Equal((int)statusCode, response.ResponseCode);
            Assert.Equal($"Test message for {statusCode}", response.Message);
        }
    }

    [Fact]
    public async Task Controllers_HandleComplexErrorScenarios()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
            .ThrowsAsync(new TimeoutException("Database timeout occurred"));

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
        Assert.Equal("Database timeout occurred", response.Message);
        Assert.Equal(500, response.ResponseCode);
    }

    [Fact]
    public async Task Controllers_HandleNullResponseFromMediator()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
            .ReturnsAsync((GetPeopleResult)null!);

        var controller = new PersonController(mediator.Object);

        // Act
        var result = await controller.GetPeople();

        // Assert - The controller should handle the null gracefully and return an error response
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
        
        var response = objectResult?.Value as BaseResponse;
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(500, response.ResponseCode);
        // The exception message should indicate a null reference was encountered
        Assert.Contains("Object reference not set to an instance of an object", response.Message);
    }
}