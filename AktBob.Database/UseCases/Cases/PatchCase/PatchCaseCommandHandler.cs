using AktBob.Database.Entities;
using AktBob.Database.UseCases.Cases.GetCaseById;
using Ardalis.Result;
using Dapper;
using MediatR;

namespace AktBob.Database.UseCases.Cases.PatchCase;
internal class PatchCaseCommandHandler : IRequestHandler<PatchCaseCommand, Result<Case>>
{
    private readonly IMediator _mediator;
    private readonly ISqlDataAccess _sqlDataAccess;

    public PatchCaseCommandHandler(IMediator mediator, ISqlDataAccess sqlDataAccess)
    {
        _mediator = mediator;
        _sqlDataAccess = sqlDataAccess;
    }

    public async Task<Result<Case>> Handle(PatchCaseCommand request, CancellationToken cancellationToken)
    {
        var getCaseQuery = new GetCaseByIdQuery(request.Id);
        var getCaseResult = await _mediator.Send(getCaseQuery, cancellationToken);

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
