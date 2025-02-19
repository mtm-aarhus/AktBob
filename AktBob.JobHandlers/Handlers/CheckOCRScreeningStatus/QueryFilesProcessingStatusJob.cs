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

        // Query each file with a 10 seconds delay between queries
        // Wait for all files to return a 'finished' state
        await Task.WhenAll(
            @case.Files.Select(
                async fileId =>
                {
                    var parameters = new FileProcessStatusFileParameters
                    {
                        FileId = fileId
                    };

                    while (true)
                    {
                        await Task.Delay(10000);

                        var response = await _filArkivCoreClient.GetFileProcessStatusFileAsync(parameters);
                        _logger.LogInformation("File {fileId} IsBeingProcessed: {isBeingProcessed} ('{fileName}')", fileId, response.IsBeingProcessed, response.FileName);

                        if (!response.IsInQueue)
                        {
                            if (!response.FileProcessStatusResponses.Any(x =>
                                x.Ignore == false
                                && x.FinishedAt == null
                                && x.RetryInProgress == false))
                            {
                                break;
                            }
                        }

                        // TODO: Maybe break out of while loop after a maximum time period?
                    }
                }
        ));

        _logger.LogInformation("Finished querying processing statusses for files for FilArkiv Case {id}, PodioItemId {podioItemId}", @case.FilArkivCaseId, @case.PodioItemId);

        _cachedData.Cases.TryRemove(cacheId, out Case? removedCase);

        if (!_settings.UpdatePodioItemImmediately)
        {
            BackgroundJob.Enqueue<UpdatePodioItemJob>(job => job.Run(@case.FilArkivCaseId, @case.PodioItemId, CancellationToken.None));
        }

        BackgroundJob.Enqueue<PostPodioItemCommentJob>(job => job.Run(@case.PodioItemId, CancellationToken.None));
    }
}