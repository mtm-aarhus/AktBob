using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Cases.GetCases;
using Ardalis.GuardClauses;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AktBob.Database.UseCases.Cases;
internal class GetCasesQueryHandler(IConfiguration configuration) : IQueryHandler<GetCasesQuery, Result<IEnumerable<CaseDto>>>
{
    private readonly IConfiguration _configuration = configuration;
    public async Task<Result<IEnumerable<CaseDto>>> Handle(GetCasesQuery request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        using (var connection = new SqlConnection(connectionString))
        {
            var filter = new List<string>();

            if (request.DeskproId != null)
            {
                filter.Add($"{Constants.V_TICKETS}.{Constants.T_TICKETS_DESKPRO_ID} = {request.DeskproId.ToString()}");
            }

            if (request.PodioItemId != null)
            {
                filter.Add($"{Constants.V_CASES}.{Constants.T_CASES_PODIO_ITEM_ID} = {request.PodioItemId.ToString()}");
            }

            if (request.FilArkivCaseId != null)
            {
                filter.Add($"{Constants.V_CASES}.{Constants.T_CASES_FILARKIV_CASE_ID} = '{request.FilArkivCaseId.ToString()}'");
            }

            var filterString = string.Join(" AND ", filter);
            var getTicketIdsSql = @$"SELECT {Constants.V_CASES}.* FROM {Constants.V_CASES}
                LEFT JOIN {Constants.V_TICKETS} ON {Constants.V_CASES}.{Constants.T_CASES_TICKET_ID} = {Constants.V_TICKETS}.{Constants.T_TICKETS_ID}";

            if (filterString.Length != 0)
            {
                getTicketIdsSql += $" WHERE {filterString}";
            }

            var cases = await connection.QueryAsync<Case>(sql: getTicketIdsSql, commandType: System.Data.CommandType.Text);

            if (cases != null)
            {
                return Result.Success(cases.Select(c => c.ToDto()));
            }

            return Result.Success(Enumerable.Empty<CaseDto>());
        }
    }
}
