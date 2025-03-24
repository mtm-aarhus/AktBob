using AktBob.Podio.Contracts;
using AktBob.Shared.Extensions;
using FilArkivCore.Web.Client;
using FilArkivCore.Web.Shared.FileProcess;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

internal record QueryFilesProcessingStatusJob(Guid FilArkivCaseId);

internal class QueryFilesProcessingStatus(ILogger<QueryFilesProcessingStatusJob> logger, IServiceScopeFactory serviceProviderFactory, IConfiguration configuration, ITimeProvider timeProvider) : IJobHandler<QueryFilesProcessingStatusJob>
{
    private readonly ILogger<QueryFilesProcessingStatusJob> _logger = logger;
    private readonly IServiceScopeFactory _serviceProviderFactory = serviceProviderFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ITimeProvider _timeProvider = timeProvider;

    public async Task Handle(QueryFilesProcessingStatusJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceProviderFactory.CreateScope();
        var podio = scope.ServiceProvider.GetRequiredServiceOrThrow<IPodioModule>();
        var filArkivCoreClient = scope.ServiceProvider.GetRequiredServiceOrThrow<FilArkivCoreClient>();
        var pollingInterval = TimeSpan.FromSeconds(_configuration.GetValue<int?>("CheckOCRScreeningStatus:PollingIntervalMinutes") ?? 30);
        var cachedData = CachedData.Instance;

        if (!cachedData.Cases.TryGetValue(job.FilArkivCaseId, out var @case))
        {
            _logger.LogDebug("Case not foud in cache. Exiting job.");
            return;
        }

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
                    await _timeProvider.Delay(pollingInterval);

                    var response = await filArkivCoreClient.GetFileProcessStatusFileAsync(parameters);
                    if (!response.IsInQueue)
                    {
                        if (!response.FileProcessStatusResponses.Any(x =>
                            x.Ignore == false
                            && x.FinishedAt == null
                            && x.RetryInProgress == false))
                        {
                            _logger.LogInformation("Case {caseId} File {fileId} finished ('{fileName}')", @case.FilArkivCaseId, fileId, response.FileName);
                            break;
                        }
                    }
                }
            }
        ).ToArray();

        var timeout = TimeSpan.FromHours(_configuration.GetValue<int?>("CheckOCRScreeningStatus:QueryFilesTimeoutHours") ?? 24);
        var timeoutTask = _timeProvider.Delay(timeout);
        var completedTasks = await Task.WhenAny(Task.WhenAll(tasks), timeoutTask);

        if (completedTasks == timeoutTask)
        {
            throw new TimeoutException($"The {timeout.Hours} hour timeout has been reached for querying FilArkiv file statusses (FilArkivCaseId {job.FilArkivCaseId}).");
        }

        _logger.LogInformation("Finished querying processing statusses for all files, FilArkiv case {id}, PodioItemId {podioItemId}", @case.FilArkivCaseId, @case.PodioItemId);

        cachedData.Cases.TryRemove(job.FilArkivCaseId, out Case? removedCase);

        if (!Settings.ShouldUpdatePodioItemImmediately(_configuration))
        {
            UpdatePodioField.SetFilArkivCaseId(podio, _configuration, @case.FilArkivCaseId, @case.PodioItemId);
        }

        var commentText = "OCR screening af dokumenterne i FilArkiv er færdig.";
        var postCommandCommand = new PostCommentCommand(@case.PodioItemId, commentText);
        podio.PostComment(postCommandCommand);
    }
}