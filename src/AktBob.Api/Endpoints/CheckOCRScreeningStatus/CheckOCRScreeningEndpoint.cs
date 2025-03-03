using AktBob.Database.Contracts;
using AktBob.Shared;
using AktBob.Shared.Jobs;
using FastEndpoints;

namespace AktBob.Api.Endpoints.CheckOCRScreeningStatus;

internal class CheckOCRScreeningEndpoint(IJobDispatcher jobDispatcher,
                                         ICaseRepository caseRepository,
                                         ILogger<CheckOCRScreeningEndpoint> logger) : Endpoint<CheckOCRScreeningRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;
    private readonly ICaseRepository _caseRepository = caseRepository;
    private readonly ILogger<CheckOCRScreeningEndpoint> _logger = logger;

    public override void Configure()
    {
        Post("/Jobs/CheckOCRScreeningStatus", "/CheckOCRScreeningStatus/Case");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Initiates a new job checking the OCR screening status for a FilArkiv case's documents and updates the Podio item with the FilArkiv case ID/URL";
        });
    }

    public override async Task HandleAsync(CheckOCRScreeningRequest req, CancellationToken ct)
    {
        var job = new CheckOCRScreeningStatusRegisterFilesJob(req.FilArkivCaseId, req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        
        await UpdateDatabaseSetFilArkivCaseId(req.FilArkivCaseId, req.PodioItemId, ct);
        await SendNoContentAsync();
    }

    private async Task UpdateDatabaseSetFilArkivCaseId(Guid filArkivCaseId, long podioItemId, CancellationToken cancellationToken)
    {
        var @case = await _caseRepository.GetByPodioItemId(podioItemId);

        if (@case is null )
        {
            _logger.LogWarning("Error updating database with FilArkivCaseId. Database did not return a case for Podio item id {id}", podioItemId);
            return;
        }

        @case.FilArkivCaseId = filArkivCaseId;

        var updated = await _caseRepository.Update(@case) == 1;

        if (!updated)
        {
            _logger.LogWarning("Error updating database setting FilArkivCaseId {caseId} for row {id}", filArkivCaseId, @case.Id);
            return;
        }

        _logger.LogInformation("Database updated: FilArkivCaseId '{filArkivCaseId}' set for case with PodioItemId {podioItemId}", filArkivCaseId, podioItemId);
    }
}
