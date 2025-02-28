using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using System.Data;

namespace AktBob.Database.UseCases.Cases;

internal record GetCaseByIdQuery(int Id) : IQuery<Result<CaseDto>>;
internal class GetCaseByIdQueryHandler(ISqlDataAccess sqlDataAccess) : IQueryHandler<GetCaseByIdQuery, Result<CaseDto>>
{
    private readonly ISqlDataAccess _sqlDataAccess = sqlDataAccess;

    public async Task<Result<CaseDto>> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
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

        return result.Value.First().ToDto();
    }
}