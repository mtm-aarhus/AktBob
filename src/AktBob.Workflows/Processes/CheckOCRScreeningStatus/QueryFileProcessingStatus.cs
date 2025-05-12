using AktBob.Podio.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;
using FilArkivCore.Web.Client;
using FilArkivCore.Web.Shared.FileProcess;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

internal class QueryFileProcessingStatus : IJobHandler<QueryFileProcessingStatusJob>
{
    private readonly ILogger<QueryFileProcessingStatusJob> _logger;
    private readonly IServiceScopeFactory _serviceProviderFactory;
    private readonly IConfiguration _configuration;
    private readonly IJobDispatcher _jobDispatcher;

    public QueryFileProcessingStatus(
        ILogger<QueryFileProcessingStatusJob> logger,
        IServiceScopeFactory serviceProviderFactory,
        IConfiguration configuration,
        IJobDispatcher jobDispatcher)
    {
        _logger = logger;
        _serviceProviderFactory = serviceProviderFactory;
        _configuration = configuration;
        _jobDispatcher = jobDispatcher;
    }

    public async Task Handle(QueryFileProcessingStatusJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProviderFactory.CreateScope();
        var podio = scope.ServiceProvider.GetRequiredServiceOrThrow<IPodioModule>();
        var filArkivCoreClient = scope.ServiceProvider.GetRequiredServiceOrThrow<FilArkivCoreClient>();

        // Get cached data
        if (!CachedData.Instance.Cases.TryGetValue(job.FilArkivCaseId, out var @case))
        {
            _logger.LogDebug("FilArkivCase not found in cache. Reinitializing CheckOCRScreeningStatus job for Podio {itemId} FilArkivCase {caseId}.", job.PodioItemId, job.FilArkivCaseId);
            _jobDispatcher.Dispatch(new CheckOCRScreeningStatusRegisterFilesJob(job.FilArkivCaseId, job.PodioItemId));
            return;
        }

        // Ensure file is in the cache
        var file = @case.GetFile(job.FilArkivFileId);
        if (file == null)
        {
            _logger.LogDebug("FilArkivFile {id} not found in cache. Reinitializing CheckOCRScreeningStatus job for Podio {itemId} FilArkivCase {caseId}.", job.FilArkivFileId, job.PodioItemId, job.FilArkivCaseId);
            _jobDispatcher.Dispatch(new CheckOCRScreeningStatusRegisterFilesJob(job.FilArkivCaseId, job.PodioItemId));
            return;
        }

        // If the file has already been checked, try if notifying can be done and then exit early
        if (file.IsFinished)
        {
            NotifyWhenAllFilesAreFinished(job.PodioItemId, @case);
            return;
        }

        // Get current status from FilArkiv
        var response = await filArkivCoreClient.GetFileProcessStatusFileAsync(new FileProcessStatusFileParameters { FileId = job.FilArkivFileId });
        if (response.IsInQueue || response.IsBeingProcessed)
        {
            // File not finished yet - reschedule
            RescheduleFileStatusQuery(job);
            return;
        }

        // Finished - update cache
        _logger.LogInformation("Case {caseId} File {fileId} finished ('{fileName}')", @case.FilArkivCaseId, job.FilArkivFileId, response.FileName);
        file.SetStatus(true);

        // Notify if all files are finished
        NotifyWhenAllFilesAreFinished(job.PodioItemId, @case);
    }

    private void RescheduleFileStatusQuery(QueryFileProcessingStatusJob job)
    {
        var delay = TimeSpan.FromSeconds(Math.Clamp(10 + Math.Pow(job.Count, 2), 0, 600));
        _logger.LogDebug("OCR-screening not finished yet, FilArkivCase {caseId} FilArkivFile {fileId}. Retry in {delay}", job.FilArkivCaseId, job.FilArkivFileId, delay);
        _jobDispatcher.Dispatch(job with { Count = job.Count + 1 }, delay);
    }

    private void NotifyWhenAllFilesAreFinished(PodioItemId podioItemId, Case @case)
    {
        // Not all files are finished - exit
        if (@case.AnyFilesNotFinished)
        {
            return;
        }

        // All files are finished
        _logger.LogInformation("FilArkiv case {id}, PodioItemId {podioItemId}: all files finished OCR screening", @case.FilArkivCaseId, @case.PodioItemId);

        // Remove case from cache
        CachedData.Instance.Cases.TryRemove(@case.FilArkivCaseId, out Case? removedCase);

        // Dispatch notification jobs
        if (!Settings.ShouldUpdatePodioItemImmediately(_configuration))
        {
            _jobDispatcher.Dispatch(new UpdatePodioFilArkivFieldsJob(podioItemId, @case.FilArkivCaseId));
        }
        _jobDispatcher.Dispatch(new ScreeningIsFinishedEmailNotificationJob(@case.PodioItemId, @case.FilArkivCaseId));
        _jobDispatcher.Dispatch(new ScreeningIsFinishedPodioNotificationJob(@case.PodioItemId));
    }
}