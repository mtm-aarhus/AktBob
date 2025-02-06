using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using Ardalis.Result;
using Dapper;
using MassTransit.Mediator;
using System.Data;

namespace AktBob.Database.UseCases.Cases;

public record GetCasesByTicketIdQuery(int TicketId) : Request<Result<IEnumerable<CaseDto>>>;

public class GetCasesByTicketIdQueryHandler(ISqlDataAccess sqlDataAccess) : MediatorRequestHandler<GetCasesByTicketIdQuery, Result<IEnumerable<CaseDto>>>
{
    private readonly ISqlDataAccess _sqlDataAccess = sqlDataAccess;

    protected override async Task<Result<IEnumerable<CaseDto>>> Handle(GetCasesByTicketIdQuery request, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add(Constants.T_CASES_TICKET_ID, request.TicketId, DbType.Int32, ParameterDirection.Input);

        var result = await _sqlDataAccess.ExecuteProcedure<Case>(Constants.SP_CASE_GET_BY_TICKET_ID, parameters);
        return Result.Success(result.Value.Select(x => x.ToDto()));
    }
}