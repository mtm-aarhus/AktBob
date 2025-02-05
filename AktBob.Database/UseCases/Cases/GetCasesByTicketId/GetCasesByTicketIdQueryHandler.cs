using AktBob.Database.Entities;
using Ardalis.Result;
using Dapper;
using MediatR;
using System.Data;

namespace AktBob.Database.UseCases.Cases.GetCasesByTicketId;
internal class GetCasesByTicketIdQueryHandler : IRequestHandler<GetCasesByTicketIdQuery, Result<IEnumerable<Case>>>
{
    private readonly ISqlDataAccess _sqlDataAccess;

    public GetCasesByTicketIdQueryHandler(ISqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    public async Task<Result<IEnumerable<Case>>> Handle(GetCasesByTicketIdQuery request, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add(Constants.T_CASES_TICKET_ID, request.TicketId, DbType.Int32, ParameterDirection.Input);

        var result = await _sqlDataAccess.ExecuteProcedure<Case>(Constants.SP_CASE_GET_BY_TICKET_ID, parameters);
        return result;
    }
}