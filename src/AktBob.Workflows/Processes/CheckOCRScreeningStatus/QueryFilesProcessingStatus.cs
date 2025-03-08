using AktBob.Podio.Contracts;
using FilArkivCore.Web.Client;
using FilArkivCore.Web.Shared.FileProcess;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

internal record QueryFilesProcessingStatusJob(Guid CacheId);

internal class QueryFilesProcessingStatus(ILogger<QueryFilesProcessingStatusJob> logger, IServiceScopeFactory serviceProviderFactory, IConfiguration configuration) : IJobHandler<QueryFilesProcessingStatusJob>
{
    private readonly ILogger<QueryFilesProcessingStatusJob> _logger = logger;
    private readonly IServiceScopeFactory _serviceProviderFactory = serviceProviderFactory;
    private readonly IConfiguration _configuration = configuration;

    public Task Handle(QueryFilesProcessingStatusJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceProviderFactory.CreateScope();
        var podio = scope.ServiceProvider.GetRequiredService<IPodioModule>();
        var filArkivCoreClient = scope.ServiceProvider.GetRequiredService<FilArkivCoreClient>();
        var cachedData = scope.ServiceProvider.GetRequiredService<CachedData>();
        var settings = scope.ServiceProvider.GetRequiredService<Settings>();

        if (!cachedData.Cases.TryGetValue(job.CacheId, out var @case))
        {
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

                while (true)
                {
                    var delay = 30000 + random.Next(-2000, 2000);
                    await Task.Delay(delay);

                    var response = await filArkivCoreClient.GetFileProcessStatusFileAsync(parameters);

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
            UpdatePodioField.SetFilArkivCaseId(podio, _configuration, @case.FilArkivCaseId, @case.PodioItemId);
        }

        var commentText = "OCR screening af dokumenterne i FilArkiv er færdig.";

        var postCommandCommand = new PostCommentCommand(@case.PodioItemId, commentText);
        podio.PostComment(postCommandCommand);

        _logger.LogDebug("Executed {name} with {job}", nameof(QueryFilesProcessingStatus), job);

        return Task.CompletedTask;
    }
}