using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class UpdateAstronautDuty : IRequest<UpdateAstronautDutyResult>
    {
        public int DutyId { get; set; }
        public string? DutyTitle { get; set; }
        public string? Rank { get; set; }
        public DateTime? DutyStartDate { get; set; }
        public DateTime? DutyEndDate { get; set; }
    }

    public class UpdateAstronautDutyResult : BaseResponse
    {
        public AstronautDuty? AstronautDuty { get; set; }
        public bool RetirementDutyCreated { get; set; }
    }

    public class UpdateAstronautDutyHandler : IRequestHandler<UpdateAstronautDuty, UpdateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public UpdateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<UpdateAstronautDutyResult> Handle(UpdateAstronautDuty request, CancellationToken cancellationToken)
        {
            var result = new UpdateAstronautDutyResult();
            bool retirementDutyCreated = false;

            // Find the existing duty
            var existingDuty = await _context.AstronautDuties
                .FirstOrDefaultAsync(d => d.Id == request.DutyId, cancellationToken);

            if (existingDuty == null)
            {
                result.Success = false;
                result.Message = "Astronaut duty not found";
                result.ResponseCode = 404;
                return result;
            }

            // Update the duty fields if provided
            if (!string.IsNullOrEmpty(request.DutyTitle))
                existingDuty.DutyTitle = request.DutyTitle;
            
            if (!string.IsNullOrEmpty(request.Rank))
                existingDuty.Rank = request.Rank;
            
            if (request.DutyStartDate.HasValue)
                existingDuty.DutyStartDate = request.DutyStartDate.Value;

            // Handle end date logic
            if (request.DutyEndDate.HasValue)
            {
                existingDuty.DutyEndDate = request.DutyEndDate.Value;

                // Check if there are any other open duties for this person
                var otherOpenDuties = await _context.AstronautDuties
                    .Where(d => d.PersonId == existingDuty.PersonId && 
                               d.Id != existingDuty.Id && 
                               d.DutyEndDate == null)
                    .CountAsync(cancellationToken);

                // If no other open duties, automatically create a retirement duty
                if (otherOpenDuties == 0)
                {
                    var retirementDuty = new AstronautDuty
                    {
                        PersonId = existingDuty.PersonId,
                        DutyTitle = "RETIRED",
                        Rank = existingDuty.Rank, // Keep the same rank
                        DutyStartDate = request.DutyEndDate.Value.AddDays(1).Date, // Start retirement the day after duty ends
                        DutyEndDate = null // Retirement has no end date
                    };

                    await _context.AstronautDuties.AddAsync(retirementDuty, cancellationToken);

                    // Update AstronautDetail with retirement info
                    var astronautDetail = await _context.AstronautDetails
                        .FirstOrDefaultAsync(ad => ad.PersonId == existingDuty.PersonId, cancellationToken);

                    if (astronautDetail != null)
                    {
                        astronautDetail.CurrentDutyTitle = "RETIRED";
                        astronautDetail.CurrentRank = existingDuty.Rank;
                        astronautDetail.CareerEndDate = request.DutyEndDate.Value.Date;
                        _context.AstronautDetails.Update(astronautDetail);
                    }

                    retirementDutyCreated = true;
                }
            }

            // Update the original duty
            _context.AstronautDuties.Update(existingDuty);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                
                result.Success = true;
                result.Message = retirementDutyCreated 
                    ? "Astronaut duty updated and retirement duty created successfully"
                    : "Astronaut duty updated successfully";
                result.ResponseCode = 200;
                result.AstronautDuty = existingDuty;
                result.RetirementDutyCreated = retirementDutyCreated;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error updating astronaut duty: {ex.Message}";
                result.ResponseCode = 500;
            }

            return result;
        }
    }
}