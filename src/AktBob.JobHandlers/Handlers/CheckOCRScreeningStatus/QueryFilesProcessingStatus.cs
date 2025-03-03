using AktBob.Podio.Contracts.Jobs;
using FilArkivCore.Web.Client;
using FilArkivCore.Web.Shared.FileProcess;

namespace AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;

internal record QueryFilesProcessingStatusJob(Guid CacheId);

internal class QueryFilesProcessingStatus(ILogger<QueryFilesProcessingStatusJob> logger, IServiceScopeFactory serviceProviderFactory, IConfiguration configuration) : IJobHandler<QueryFilesProcessingStatusJob>
{
    private readonly ILogger<QueryFilesProcessingStatusJob> _logger = logger;
    private readonly IServiceScopeFactory _serviceProviderFactory = serviceProviderFactory;
    private readonly IConfiguration _configuration = configuration;

    public Task Handle(QueryFilesProcessingStatusJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceProviderFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();
        var filArkivCoreClient = scope.ServiceProvider.GetRequiredService<FilArkivCoreClient>();
        var cachedData = scope.ServiceProvider.GetRequiredService<CachedData>();
        var settings = scope.ServiceProvider.GetRequiredService<Settings>();

        if (!cachedData.Cases.TryGetValue(job.CacheId, out var @case))
        {
            _logger.LogWarning("Cached case not found");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Querying processing statusses for files for FilArkiv Case {id}, PodioItemId {podioItemId}", @case.FilArkivCaseId, @case.PodioItemId);

        // Query each file with a delay between queries
        // Wait for all files to return a 'finished' state
        var random = new Random();
        var tasks = @case.Files.Select(
            async fileId =>
            {
                var parameters = new FileProcessStatusFileParameters
                {
                    FileId = fileId
                };

                var first = true;

                while (true)
                {
                    var delay = 20000 + random.Next(-2000, 2000);
                    await Task.Delay(delay);

                    var response = await filArkivCoreClient.GetFileProcessStatusFileAsync(parameters);

                    if (first)
                    {
                        _logger.LogInformation("Case {caseId} File {fileId} IsBeingProcessed: {isBeingProcessed} IsInQueue: {isInQueue} ({queueNumber}) ('{fileName}')", @case.FilArkivCaseId, fileId, response.IsBeingProcessed, response.IsInQueue, response.QueueNumber, response.FileName);
                        first = false;
                    }

                    if (!response.IsInQueue)
                    {
                        if (!response.FileProcessStatusResponses.Any(x =>
                            x.Ignore == false
                            && x.FinishedAt == null
                            && x.RetryInProgress == false))
                        {
                            _logger.LogInformation("Case {caseId} File {fileId} Finished ('{fileName}')", @case.FilArkivCaseId, fileId, response.FileName);
                            break;
                        }
                    }

                    // TODO: Maybe break out of while loop after a maximum time period?
                }
            }
        ).ToArray();

        Task.WaitAll(tasks, cancellationToken);

        _logger.LogInformation("Finished querying processing statusses for files for FilArkiv Case {id}, PodioItemId {podioItemId}", @case.FilArkivCaseId, @case.PodioItemId);

        cachedData.Cases.TryRemove(job.CacheId, out Case? removedCase);

        if (!settings.UpdatePodioItemImmediately)
        {
            UpdatePodioField.SetFilArkivCaseId(jobDispatcher, _configuration, @case.FilArkivCaseId, @case.PodioItemId);
        }

        var podioAppId = Guard.Against.Null(_configuration.GetValue<int>("Podio:AppId"));
        var commentText = "OCR screening af dokumenterne i FilArkiv er færdig.";

        jobDispatcher.Dispatch(new PostCommentJob(podioAppId, @case.PodioItemId, commentText));

        return Task.CompletedTask;
    }
}