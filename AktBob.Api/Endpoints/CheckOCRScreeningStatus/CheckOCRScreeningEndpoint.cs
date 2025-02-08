using AktBob.Database.Contracts;
using AktBob.Database.UseCases.Cases.GetCases;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;

namespace AktBob.Api.Endpoints.CheckOCRScreeningStatus;

internal class CheckOCRScreeningEndpoint(IJobDispatcher jobDispatcher, IMediator mediator, ILogger<CheckOCRScreeningEndpoint> logger) : Endpoint<CheckOCRScreeningRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<CheckOCRScreeningEndpoint> _logger = logger;

    public override void Configure()
    {
        Post("/Jobs/CheckOCRScreeningStatus", "/CheckOCRScreeningStatus/Case");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Initiate a new job checking the OCR screening status for a FilArkiv case's documents";
        });
    }

    public override async Task HandleAsync(CheckOCRScreeningRequest req, CancellationToken ct)
    {
        var job = new CheckOCRScreeningStatusJob(req.FilArkivCaseId, req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        
        await UpdateDatabaseSetFilArkivCaseId(req.FilArkivCaseId, req.PodioItemId, ct);
        await SendNoContentAsync();
    }

    private async Task UpdateDatabaseSetFilArkivCaseId(Guid filArkivCaseId, long podioItemId, CancellationToken cancellationToken)
    {
        var getCaseQuery = new GetCasesQuery(null, podioItemId, null);
        var getCaseResult = await _mediator.SendRequest(getCaseQuery, cancellationToken);

        if (!getCaseResult.IsSuccess || !getCaseResult.Value.Any())
        {
            _logger.LogWarning("Error updating database with FilArkivCaseId. Database did not return a case for Podio item id {id}", podioItemId);
            return;
        }

        var rowId = getCaseResult.Value.First().Id;

        var updateCommand = new UpdateCaseCommand(rowId, podioItemId, null, filArkivCaseId, null);
        var updateResult = await _mediator.SendRequest(updateCommand, cancellationToken);

        if (!updateResult.IsSuccess)
        {
            _logger.LogWarning("Error updating database setting FilArkivCaseId {caseId} for row {id}", filArkivCaseId, rowId);
            return;
        }

        _logger.LogInformation("Database updated: FilArkivCaseId '{filArkivCaseId}' set for case with PodioItemId {podioItemId}", filArkivCaseId, podioItemId);

    }
}
