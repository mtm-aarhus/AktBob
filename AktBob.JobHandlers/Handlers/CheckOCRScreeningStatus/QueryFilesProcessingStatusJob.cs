using FilArkivCore.Web.Client;
using FilArkivCore.Web.Shared.FileProcess;

namespace AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;
internal class QueryFilesProcessingStatusJob(ILogger<QueryFilesProcessingStatusJob> logger,
                                             FilArkivCoreClient filArkivCoreClient,
                                             CachedData cachedData,
                                             CheckOCRScreeningStatusSettings settings)
{
    private readonly ILogger<QueryFilesProcessingStatusJob> _logger = logger;
    private readonly FilArkivCoreClient _filArkivCoreClient = filArkivCoreClient;
    private readonly CachedData _cachedData = cachedData;
    private readonly CheckOCRScreeningStatusSettings _settings = settings;

    public async Task Run(Guid cacheId, CancellationToken cancellationToken = default)
    {
        if (!_cachedData.Cases.TryGetValue(cacheId, out var @case))
        {
            _logger.LogWarning("Cached case not found");
            return;
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

                    var response = await _filArkivCoreClient.GetFileProcessStatusFileAsync(parameters);

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

        _cachedData.Cases.TryRemove(cacheId, out Case? removedCase);

        if (!_settings.UpdatePodioItemImmediately)
        {
            BackgroundJob.Enqueue<UpdatePodioItemJob>(job => job.Run(@case.FilArkivCaseId, @case.PodioItemId, CancellationToken.None));
        }

        BackgroundJob.Enqueue<PostPodioItemCommentJob>(job => job.Run(@case.PodioItemId, CancellationToken.None));
    }
}