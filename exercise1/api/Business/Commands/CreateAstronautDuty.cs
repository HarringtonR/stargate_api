using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            // Validate person exists (Rule #1 enforcement)
            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);
            if (person is null) 
                throw new BadHttpRequestException($"Person with name '{request.Name}' not found");

            // Business Rule: A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time
            // Check if person already has a duty with the same start date
            var existingDutyOnSameDate = _context.AstronautDuties
                .AsNoTracking()
                .FirstOrDefault(z => z.PersonId == person.Id && z.DutyStartDate.Date == request.DutyStartDate.Date);

            if (existingDutyOnSameDate is not null)
                throw new BadHttpRequestException($"Person '{request.Name}' already has an astronaut duty starting on {request.DutyStartDate:yyyy-MM-dd}. Only one duty per start date is allowed.");

            // Business Rule: Start date should be 1 day after the end date of previous duty (except for RETIRED)
            var lastEndedDuty = _context.AstronautDuties
                .AsNoTracking()
                .Where(z => z.PersonId == person.Id && z.DutyEndDate.HasValue)
                .OrderByDescending(z => z.DutyEndDate)
                .FirstOrDefault();

            if (lastEndedDuty != null && lastEndedDuty.DutyEndDate.HasValue && request.DutyTitle.ToUpper() != "RETIRED")
            {
                var expectedStartDate = lastEndedDuty.DutyEndDate.Value.AddDays(1).Date;
                if (request.DutyStartDate.Date != expectedStartDate)
                {
                    throw new BadHttpRequestException(
                        $"New duty start date should be {expectedStartDate:yyyy-MM-dd} " +
                        $"(one day after the previous duty end date {lastEndedDuty.DutyEndDate.Value:yyyy-MM-dd}). " +
                        $"Current start date: {request.DutyStartDate:yyyy-MM-dd}");
                }
            }

            // Business Rule: Check if person has any current (open) duties
            var currentOpenDuty = _context.AstronautDuties
                .AsNoTracking()
                .FirstOrDefault(z => z.PersonId == person.Id && z.DutyEndDate == null);

            if (currentOpenDuty != null && request.DutyTitle.ToUpper() != "RETIRED")
            {
                // If there's an open duty, the new duty should start the day after we close the current one
                var expectedStartDate = request.DutyStartDate.AddDays(-1).Date.AddDays(1);
                if (request.DutyStartDate.Date != expectedStartDate)
                {
                    throw new BadHttpRequestException(
                        $"Person '{request.Name}' has an open duty that will be closed on {request.DutyStartDate.AddDays(-1):yyyy-MM-dd}. " +
                        $"New duty should start on {request.DutyStartDate:yyyy-MM-dd}.");
                }
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Rank))
                throw new BadHttpRequestException("Rank is required");

            if (string.IsNullOrWhiteSpace(request.DutyTitle))
                throw new BadHttpRequestException("Duty Title is required");

            if (request.DutyStartDate == default(DateTime))
                throw new BadHttpRequestException("Duty Start Date is required");

            return Task.CompletedTask;
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {

            var query = $"SELECT * FROM [Person] WHERE \'{request.Name}\' = Name";

            var person = await _context.Connection.QueryFirstOrDefaultAsync<Person>(query);

            if (person == null)
            {
                throw new BadHttpRequestException("Person not found");
            }

            query = $"SELECT * FROM [AstronautDetail] WHERE {person.Id} = PersonId";

            var astronautDetail = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDetail>(query);

            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail();
                astronautDetail.PersonId = person.Id;
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                astronautDetail.CareerStartDate = request.DutyStartDate.Date;
                if (request.DutyTitle.ToUpper() == "RETIRED")
                {
                    // Set career end date to one day before retired duty start date (same as duty end dates)
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }

                await _context.AstronautDetails.AddAsync(astronautDetail);

            }
            else
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                if (request.DutyTitle.ToUpper() == "RETIRED")
                {
                    // Set career end date to one day before retired duty start date (same as duty end dates)
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }
                _context.AstronautDetails.Update(astronautDetail);
            }

            // When adding a RETIRED duty, close all existing open duties for this person
            if (request.DutyTitle.ToUpper() == "RETIRED")
            {
                // Get all existing duties for this person that don't have an end date
                var openDuties = await _context.AstronautDuties
                    .Where(d => d.PersonId == person.Id && d.DutyEndDate == null)
                    .ToListAsync();

                // Set the end date to one day before the retired duty start date
                var endDate = request.DutyStartDate.AddDays(-1).Date;
                foreach (var duty in openDuties)
                {
                    duty.DutyEndDate = endDate;
                    _context.AstronautDuties.Update(duty);
                }
            }
            else
            {
                // For non-retired duties, only close the most recent duty as before
                query = $"SELECT * FROM [AstronautDuty] WHERE {person.Id} = PersonId AND DutyEndDate IS NULL Order By DutyStartDate Desc";

                var astronautDuty = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(query);

                if (astronautDuty != null)
                {
                    astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                    _context.AstronautDuties.Update(astronautDuty);
                }
            }

            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = request.DutyStartDate.Date,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty);

            await _context.SaveChangesAsync();

            return new CreateAstronautDutyResult()
            {
                Id = newAstronautDuty.Id
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
