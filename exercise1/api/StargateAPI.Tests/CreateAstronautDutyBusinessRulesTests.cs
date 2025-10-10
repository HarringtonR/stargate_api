using Xunit;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.AspNetCore.Http;

namespace StargateAPI.Tests;

public class CreateAstronautDutyBusinessRulesTests
{
    private StargateContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<StargateContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new StargateContext(options);
    }

    [Fact]
    public async Task CreateAstronautDuty_PreventsDuplicateStartDate_ForSamePerson()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Create test person
        var person = new Person { Id = 1, Name = "Test Person" };
        context.People.Add(person);
        
        // Create existing duty
        var existingDuty = new AstronautDuty
        {
            Id = 1,
            PersonId = 1,
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 1, 1),
            DutyEndDate = null
        };
        context.AstronautDuties.Add(existingDuty);
        await context.SaveChangesAsync();

        var preprocessor = new CreateAstronautDutyPreProcessor(context);
        var request = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Major",
            DutyTitle = "Science Officer",
            DutyStartDate = new DateTime(2024, 1, 1) // Same start date
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadHttpRequestException>(
            () => preprocessor.Process(request, CancellationToken.None));
        
        Assert.Contains("already has an astronaut duty starting on 2024-01-01", exception.Message);
    }

    [Fact]
    public async Task CreateAstronautDuty_AllowsDifferentStartDates_ForSamePerson()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Create test person
        var person = new Person { Id = 1, Name = "Test Person" };
        context.People.Add(person);
        
        // Create existing duty
        var existingDuty = new AstronautDuty
        {
            Id = 1,
            PersonId = 1,
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 1, 1),
            DutyEndDate = new DateTime(2024, 6, 30)
        };
        context.AstronautDuties.Add(existingDuty);
        await context.SaveChangesAsync();

        var preprocessor = new CreateAstronautDutyPreProcessor(context);
        var request = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Major",
            DutyTitle = "Science Officer",
            DutyStartDate = new DateTime(2024, 7, 1) // One day after end date
        };

        // Act & Assert - Should not throw exception
        await preprocessor.Process(request, CancellationToken.None);
    }

    [Fact]
    public async Task CreateAstronautDuty_EnforcesSequentialStartDates()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Create test person
        var person = new Person { Id = 1, Name = "Test Person" };
        context.People.Add(person);
        
        // Create previous duty that ended
        var previousDuty = new AstronautDuty
        {
            Id = 1,
            PersonId = 1,
            Rank = "Lieutenant",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2024, 1, 1),
            DutyEndDate = new DateTime(2024, 6, 30)
        };
        context.AstronautDuties.Add(previousDuty);
        await context.SaveChangesAsync();

        var preprocessor = new CreateAstronautDutyPreProcessor(context);
        var request = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 8, 1) // Gap after end date
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadHttpRequestException>(
            () => preprocessor.Process(request, CancellationToken.None));
        
        Assert.Contains("New duty start date should be 2024-07-01", exception.Message);
    }

    [Fact]
    public async Task CreateAstronautDuty_AllowsRetiredDutyWithoutSequentialCheck()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Create test person
        var person = new Person { Id = 1, Name = "Test Person" };
        context.People.Add(person);
        
        // Create previous duty that ended
        var previousDuty = new AstronautDuty
        {
            Id = 1,
            PersonId = 1,
            Rank = "Lieutenant",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2024, 1, 1),
            DutyEndDate = new DateTime(2024, 6, 30)
        };
        context.AstronautDuties.Add(previousDuty);
        await context.SaveChangesAsync();

        var preprocessor = new CreateAstronautDutyPreProcessor(context);
        var request = new CreateAstronautDuty
        {
            Name = "Test Person",
            Rank = "General",
            DutyTitle = "RETIRED",
            DutyStartDate = new DateTime(2024, 12, 31) // Can be any date for retirement
        };

        // Act & Assert - Should not throw exception for RETIRED duty
        await preprocessor.Process(request, CancellationToken.None);
    }

    [Fact]
    public async Task CreateAstronautDuty_ThrowsException_WhenPersonNotFound()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var preprocessor = new CreateAstronautDutyPreProcessor(context);
        
        var request = new CreateAstronautDuty
        {
            Name = "Non Existent Person",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2024, 1, 1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadHttpRequestException>(
            () => preprocessor.Process(request, CancellationToken.None));
        
        Assert.Contains("Person with name 'Non Existent Person' not found", exception.Message);
    }
}