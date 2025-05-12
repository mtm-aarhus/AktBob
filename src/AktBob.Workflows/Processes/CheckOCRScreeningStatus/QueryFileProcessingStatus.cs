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

        if (!CachedData.Instance.Cases.TryGetValue(job.FilArkivCaseId, out var @case))
        {
            _logger.LogDebug("FilArkivCase not found in cache. Reinitializing CheckOCRScreeningStatus job for Podio {itemId} FilArkivCase {caseId}.", job.PodioItemId, job.FilArkivCaseId);
            _jobDispatcher.Dispatch(new CheckOCRScreeningStatusRegisterFilesJob(job.FilArkivCaseId, job.PodioItemId));
            return;
        }

        var file = @case.Files[job.FilArkivFileId];
        var rand = new Random();
        var next = rand.Next(50);
        var response = await Task.FromResult(new FileProcessResponse { IsBeingProcessed = next >= 35, IsInQueue = next >= 35 }); // await filArkivCoreClient.GetFileProcessStatusFileAsync(new FileProcessStatusFileParameters { FileId = job.FilArkivFileId });

        if (response.IsInQueue || response.IsBeingProcessed)
        {
            // File not finished yet - reschedule
            var delay = TimeSpan.FromSeconds(Math.Clamp(10 + Math.Pow(job.Count, 2), 0, 600));
            _logger.LogDebug("OCR-screening not finished yet, FilArkivCase {caseId} FilArkivFile {fileId}. Retry in {delay}", job.FilArkivCaseId, job.FilArkivFileId, delay);
            _jobDispatcher.Dispatch(job with { Count = job.Count + 1 }, delay);
        }
        else
        {
            _logger.LogInformation("Case {caseId} File {fileId} finished ('{fileName}')", @case.FilArkivCaseId, job.FilArkivFileId, response.FileName);
            @case.Files[job.FilArkivFileId] = true;

            if (!@case.Files.Any(f => f.Value == false))
            {
                NotifyAllFilesAreDone(podio, @case);
            }
        }
    }

    private void NotifyAllFilesAreDone(IPodioModule podio, Case @case)
    {
        _logger.LogInformation("FilArkiv case {id}, PodioItemId {podioItemId}: all files finished OCR screening", @case.FilArkivCaseId, @case.PodioItemId);
        CachedData.Instance.Cases.TryRemove(@case.FilArkivCaseId, out Case? removedCase);

        if (!Settings.ShouldUpdatePodioItemImmediately(_configuration))
        {
            UpdatePodioField.SetFilArkivCaseId(podio, _configuration, @case.FilArkivCaseId, @case.PodioItemId);
        }

        PostCommentOnPodioItem(podio, @case);
        _jobDispatcher.Dispatch(new ScreeningIsFinishedNotificationJob(@case.PodioItemId, @case.FilArkivCaseId));
    }

    private static void PostCommentOnPodioItem(IPodioModule podio, Case @case)
    {
        var commentText = "Screening af dokumenterne er færdig.";
        var postCommandCommand = new PostCommentCommand(@case.PodioItemId, commentText);
        podio.PostComment(postCommandCommand);
    }
}