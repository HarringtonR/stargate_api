using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        public readonly StargateContext _context;
        public GetPeopleHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            var result = new GetPeopleResult();

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
                LEFT JOIN [AstronautDuty] c on c.PersonId = a.Id AND c.DutyEndDate IS NULL";

            var people = await _context.Connection.QueryAsync<PersonAstronaut>(query);

            result.People = people.ToList();

            return result;
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };

    }
}
