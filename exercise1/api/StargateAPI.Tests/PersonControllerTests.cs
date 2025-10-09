using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Controllers;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Threading.Tasks;

namespace StargateAPI.Tests;

public class PersonControllerTests
{
    [Fact]
    public async Task GetPeople_ReturnsObjectResult()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
            .ReturnsAsync(new GetPeopleResult { Success = true });

        var controller = new PersonController(mediator.Object);

        var result = await controller.GetPeople();

        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task GetPersonByName_ReturnsObjectResult_WhenPersonExists()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), default))
            .ReturnsAsync(new GetPersonByNameResult { Success = true });

        var controller = new PersonController(mediator.Object);

        var result = await controller.GetPersonByName("John Doe");

        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }

    [Fact]
    public async Task CreatePerson_ReturnsObjectResult_WhenPersonCreated()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreatePerson>(), default))
            .ReturnsAsync(new CreatePersonResult { Success = true, Id = 1 });

        var controller = new PersonController(mediator.Object);

        var result = await controller.CreatePerson("New Name");

        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(200, objectResult?.StatusCode);
    }
}