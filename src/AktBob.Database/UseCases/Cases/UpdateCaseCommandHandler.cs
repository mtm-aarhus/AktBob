using AktBob.Database.Contracts;
using AktBob.Database.Contracts.Dtos;

namespace AktBob.Database.UseCases.Cases;
internal class UpdateCaseCommandHandler(IQueryDispatcher queryDispatcher, ISqlDataAccess sqlDataAccess) : ICommandHandler<UpdateCaseCommand, Result<CaseDto>>
{
    private readonly IQueryDispatcher _queryDispatcher = queryDispatcher;
    private readonly ISqlDataAccess _sqlDataAccess = sqlDataAccess;

    public async Task<Result<CaseDto>> Handle(UpdateCaseCommand request, CancellationToken cancellationToken)
    {
        var getCaseQuery = new GetCaseByIdQuery(request.Id);
        var getCaseResult = await _queryDispatcher.Dispatch(getCaseQuery, cancellationToken);

        if (!getCaseResult.IsSuccess)
        {
            return getCaseResult;
        }

        var @case = getCaseResult.Value;

        // Assign new values to properties

        if (request.PodioItemId is not null)
        {
            @case.PodioItemId = (long)request.PodioItemId;
        }

        if (request.FilArkivCaseId is not null)
        {
            @case.FilArkivCaseId = request.FilArkivCaseId;
        }

        if (!string.IsNullOrEmpty(request.CaseNumber))
        {
            @case.CaseNumber = request.CaseNumber;
        }

        if (!string.IsNullOrEmpty(request.SharepointFolderName))
        {
            @case.SharepointFolderName = request.SharepointFolderName;
        }

        // Execture database procedure
        var dynamicParameters = new DynamicParameters();
        dynamicParameters.Add(Constants.T_CASES_ID, @case.Id);
        dynamicParameters.Add(Constants.T_CASES_TICKET_ID, @case.TicketId);
        dynamicParameters.Add(Constants.T_CASES_PODIO_ITEM_ID, @case.PodioItemId);
        dynamicParameters.Add(Constants.T_CASES_FILARKIV_CASE_ID, @case.FilArkivCaseId);
        dynamicParameters.Add(Constants.T_CASES_CASENUMBER, @case.CaseNumber);
        dynamicParameters.Add(Constants.T_CASES_SHAREPOINT_FOLDERNAME, @case.SharepointFolderName);

        var result = await _sqlDataAccess.ExecuteProcedure(Constants.SP_CASE_UPDATE_BY_ID, dynamicParameters);

        if (!result.IsSuccess)
        {
            return Result.CriticalError();
        }

        return Result.Success(@case);
    }
}
