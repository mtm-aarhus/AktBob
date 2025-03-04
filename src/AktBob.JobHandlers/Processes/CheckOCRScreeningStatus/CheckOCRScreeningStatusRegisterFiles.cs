using AktBob.Podio.Contracts;
using AktBob.Shared.Jobs;
using FilArkivCore.Web.Client;
using FilArkivCore.Web.Shared.Documents;

namespace AktBob.JobHandlers.Processes.CheckOCRScreeningStatus;

internal class CheckOCRScreeningStatusRegisterFiles(IServiceScopeFactory serviceScopeFactory, ILogger<CheckOCRScreeningStatusRegisterFiles> logger, IConfiguration configuration) : IJobHandler<CheckOCRScreeningStatusRegisterFilesJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CheckOCRScreeningStatusRegisterFiles> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(CheckOCRScreeningStatusRegisterFilesJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();
        var podio = scope.ServiceProvider.GetRequiredService<IPodioModule>();
        var filArkivCoreClient = scope.ServiceProvider.GetRequiredService<FilArkivCoreClient>();
        var cachedData = scope.ServiceProvider.GetRequiredService<CachedData>();
        var settings = scope.ServiceProvider.GetRequiredService<Settings>();

        var @case = new Case(job.FilArkivCaseId, job.PodioItemId);
        var cacheId = Guid.NewGuid();

        if (!cachedData.Cases.TryAdd(cacheId, @case))
        {
            _logger.LogError("Error adding case to cache");
            return;
        }

        bool moveToNextPage = true;
        int pageIndex = 0;

        while (moveToNextPage)
        {
            pageIndex++; // First page = pageIndex = 1

            var documentOverviewParameters = new DocumentOverviewParameters
            {
                CaseId = @case.FilArkivCaseId.ToString(),
                PageIndex = pageIndex,
                PageSize = 100
            };

            var documentOverview = await filArkivCoreClient.GetCaseDocumentOverviewListAsync(documentOverviewParameters);

            if (documentOverview == null)
            {
                _logger.LogWarning("FilArkiv: case {id} not found", @case.FilArkivCaseId);
                return;
            }

            if (!documentOverview.HasNextPage)
            {
                moveToNextPage = false;
            }


            // Add files to cached case object
            foreach (var document in documentOverview.Items)
            {
                var documentFileIds = document.Files.Select(f => f.Id);
                @case.Files.AddRange(documentFileIds);
            }
        }

        _logger.LogInformation("Case {caseId}: {count} files registered", @case.FilArkivCaseId, @case.Files.Count());

        // Enqueue job: query files processing status
        jobDispatcher.Dispatch(new QueryFilesProcessingStatusJob(cacheId));

        if (settings.UpdatePodioItemImmediately)
        {
            UpdatePodioField.SetFilArkivCaseId(podio, _configuration, @case.FilArkivCaseId, @case.PodioItemId);
        }
    }
}