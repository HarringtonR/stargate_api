using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;
        public GetPersonByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            var result = new GetPersonByNameResult();

            // Modified query to include people who have either AstronautDetail records OR AstronautDuty records
            var query = @"
                SELECT DISTINCT 
                    a.Id as PersonId, 
                    a.Name, 
                    COALESCE(b.CurrentRank, c.Rank) as CurrentRank,
                    COALESCE(b.CurrentDutyTitle, c.DutyTitle) as CurrentDutyTitle,
                    b.CareerStartDate, 
                    b.CareerEndDate 
                FROM [Person] a 
                LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id
                LEFT JOIN [AstronautDuty] c on c.PersonId = a.Id AND c.DutyEndDate IS NULL
                WHERE a.Name = @Name";

            var person = await _context.Connection.QueryAsync<PersonAstronaut>(query, new { Name = request.Name });

            result.Person = person.FirstOrDefault();

            return result;
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}
