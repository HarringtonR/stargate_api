using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            var result = new GetAstronautDutiesByNameResult();

            // Validate input
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                result.Success = false;
                result.Message = "Name cannot be empty or null";
                result.ResponseCode = 400; // Bad Request
                result.Person = null;
                result.AstronautDuties = new List<AstronautDuty>();
                return result;
            }

            var query = $"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE '{request.Name}' = a.Name";

            var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(query);

            // Check if person exists
            if (person == null)
            {
                result.Success = true; // This is a successful query, just no results found
                result.Message = $"No person found with name '{request.Name}'";
                result.ResponseCode = 200; // OK - successful query with no results
                result.Person = null;
                result.AstronautDuties = new List<AstronautDuty>();
                return result;
            }

            result.Person = person;

            // Now safely query for duties since we know person exists
            query = $"SELECT * FROM [AstronautDuty] WHERE {person.PersonId} = PersonId Order By DutyStartDate Desc";

            var duties = await _context.Connection.QueryAsync<AstronautDuty>(query);

            result.AstronautDuties = duties.ToList();

            // Set success response
            result.Success = true;
            result.Message = "Successfully retrieved astronaut duties";
            result.ResponseCode = 200;

            return result;
        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
