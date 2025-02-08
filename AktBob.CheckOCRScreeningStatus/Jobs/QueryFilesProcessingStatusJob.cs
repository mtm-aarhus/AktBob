using AktBob.CheckOCRScreeningStatus.Jobs;
using FilArkivCore.Web.Shared.FileProcess;
using Hangfire;

namespace AktBob.CheckOCRScreeningStatus.JobHandlers;
internal class QueryFilesProcessingStatusJob(ILogger<QueryFilesProcessingStatusJob> logger, FilArkiv filArkiv, CachedData cachedData)
{
    private readonly ILogger<QueryFilesProcessingStatusJob> _logger = logger;
    private readonly FilArkiv _filArkiv = filArkiv;
    private readonly CachedData _cachedData = cachedData;

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

                        var response = await _filArkiv.FilArkivCoreClient.GetFileProcessStatusFileAsync(parameters);
                        _logger.LogInformation("File {fileId} IsBeingProcessed: {isBeingProcessed} ('{fileName}')", fileId, response.IsBeingProcessed, response.FileName);

                        if (!response.IsBeingProcessed && !response.FileProcessStatusResponses.Any(x => x.FinishedAt == null))
                        {
                            break;
                        }

                        // TODO: Maybe break out of while loop after a maximum time period?
                    }
                }
        ));

        _logger.LogInformation("Finished querying processing statusses for files for FilArkiv Case {id}, PodioItemId {podioItemId}", @case.FilArkivCaseId, @case.PodioItemId);

        _cachedData.Cases.TryRemove(cacheId, out Case? removedCase);

        BackgroundJob.Enqueue<UpdatePodioItemJob>(job => job.Run(@case.FilArkivCaseId, @case.PodioItemId, CancellationToken.None));
        BackgroundJob.Enqueue<PostPodioItemCommentJob>(job => job.Run(@case.PodioItemId, CancellationToken.None));
    }
}