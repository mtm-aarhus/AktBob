using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using System.Data;

namespace AktBob.Database.UseCases.Cases;

internal record GetCasesByTicketIdQuery(int TicketId) : IQuery<Result<IEnumerable<CaseDto>>>;

internal class GetCasesByTicketIdQueryHandler(ISqlDataAccess sqlDataAccess) : IQueryHandler<GetCasesByTicketIdQuery, Result<IEnumerable<CaseDto>>>
{
    private readonly ISqlDataAccess _sqlDataAccess = sqlDataAccess;

    public async Task<Result<IEnumerable<CaseDto>>> Handle(GetCasesByTicketIdQuery request, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add(Constants.T_CASES_TICKET_ID, request.TicketId, DbType.Int32, ParameterDirection.Input);

        var result = await _sqlDataAccess.ExecuteProcedure<Case>(Constants.SP_CASE_GET_BY_TICKET_ID, parameters);
        return Result.Success(result?.Value?.Select(x => x.ToDto()) ?? Enumerable.Empty<CaseDto>());
    }
}