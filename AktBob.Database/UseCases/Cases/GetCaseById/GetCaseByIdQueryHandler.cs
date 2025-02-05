using AktBob.Database.Entities;
using Ardalis.Result;
using Dapper;
using MediatR;
using System.Data;

namespace AktBob.Database.UseCases.Cases.GetCaseById;
internal class GetCaseByIdQueryHandler : IRequestHandler<GetCaseByIdQuery, Result<Case>>
{
    private readonly ISqlDataAccess _sqlDataAccess;

    public GetCaseByIdQueryHandler(ISqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    public async Task<Result<Case>> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add(Constants.T_CASES_ID, request.Id, DbType.Int32, ParameterDirection.Input);

        var result = await _sqlDataAccess.ExecuteProcedure<Case>(Constants.SP_CASE_GET_BY_ID, parameters);

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.NotFound)
            {
                return Result.NotFound();
            }

            return Result.CriticalError();
        }

        return result.Value.First();
    }
}