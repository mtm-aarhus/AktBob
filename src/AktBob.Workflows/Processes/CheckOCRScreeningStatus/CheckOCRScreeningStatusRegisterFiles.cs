using AktBob.Podio.Contracts;
using AktBob.Shared.Jobs;
using FilArkivCore.Web.Client;
using FilArkivCore.Web.Shared.Documents;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

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
        var cachedData = CachedData.Instance;

        var @case = new Case(job.FilArkivCaseId, job.PodioItemId);
        var cacheId = Guid.NewGuid();

        if (!cachedData.Cases.TryAdd(cacheId, @case)) throw new BusinessException("Unable to add case to cache");

        bool moveToNextPage = true;
        int pageIndex = 1; // First page = pageIndex = 1

        while (moveToNextPage)
        {
            var documentOverviewParameters = new DocumentOverviewParameters
            {
                CaseId = @case.FilArkivCaseId.ToString(),
                PageIndex = pageIndex,
                PageSize = 100
            };

            var documentOverview = await filArkivCoreClient.GetCaseDocumentOverviewListAsync(documentOverviewParameters);
            if (documentOverview == null) throw new BusinessException("Unable to get case from FilArkiv");
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

            pageIndex++;
        }

        _logger.LogDebug("Case {caseId}: {count} files registered", @case.FilArkivCaseId, @case.Files.Count());

        // Enqueue job: query files processing status
        jobDispatcher.Dispatch(new QueryFilesProcessingStatusJob(cacheId));

        if (Settings.ShouldUpdatePodioItemImmediately(_configuration))
        {
            UpdatePodioField.SetFilArkivCaseId(podio, _configuration, @case.FilArkivCaseId, @case.PodioItemId);
        }
    }
}