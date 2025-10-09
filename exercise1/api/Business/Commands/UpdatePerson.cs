using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public required string CurrentName { get; set; } = string.Empty;
        public required string NewName { get; set; } = string.Empty;
    }

    public class UpdatePersonPreProcessor : IRequestPreProcessor<UpdatePerson>
    {
        private readonly StargateContext _context;
        public UpdatePersonPreProcessor(StargateContext context)
        {
            _context = context;
        }
        public Task Process(UpdatePerson request, CancellationToken cancellationToken)
        {
            // Check if the person with current name exists
            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.CurrentName);
            if (person is null)
            {
                throw new BadHttpRequestException($"Person with name '{request.CurrentName}' not found");
            }

            // Check if the new name is already taken by another person
            if (request.CurrentName != request.NewName)
            {
                var existingPersonWithNewName = _context.People.AsNoTracking()
                    .FirstOrDefault(z => z.Name == request.NewName);
                if (existingPersonWithNewName is not null)
                {
                    throw new BadHttpRequestException($"Person with name '{request.NewName}' already exists");
                }
            }

            return Task.CompletedTask;
        }
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;

        public UpdatePersonHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {
            var person = await _context.People.FirstOrDefaultAsync(z => z.Name == request.CurrentName, cancellationToken);
            
            if (person is null)
            {
                throw new BadHttpRequestException($"Person with name '{request.CurrentName}' not found");
            }

            person.Name = request.NewName;
            
            await _context.SaveChangesAsync(cancellationToken);

            return new UpdatePersonResult()
            {
                Id = person.Id,
                Message = "Person updated successfully",
                Success = true,
                ResponseCode = 200
            };
        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}