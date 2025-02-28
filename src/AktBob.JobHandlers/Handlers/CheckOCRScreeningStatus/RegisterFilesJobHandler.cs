using AktBob.Shared.Contracts;
using FilArkivCore.Web.Client;
using FilArkivCore.Web.Shared.Documents;

namespace AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;

internal class RegisterFilesJobHandler(IServiceScopeFactory serviceScopeFactory, ILogger<RegisterFilesJobHandler> logger) : IJobHandler<CheckOCRScreeningStatusJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<RegisterFilesJobHandler> _logger = logger;

    public async Task Handle(CheckOCRScreeningStatusJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var filArkivCoreClient = scope.ServiceProvider.GetRequiredService<FilArkivCoreClient>();
        var cachedData = scope.ServiceProvider.GetRequiredService<CachedData>();
        var settings = scope.ServiceProvider.GetRequiredService<CheckOCRScreeningStatusSettings>();

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
        BackgroundJob.Enqueue<QueryFilesProcessingStatusJob>(x => x.Run(cacheId, CancellationToken.None));

        if (settings.UpdatePodioItemImmediately)
        {
            BackgroundJob.Enqueue<UpdatePodioItemJob>(job => job.Run(@case.FilArkivCaseId, @case.PodioItemId, CancellationToken.None));
        }
    }
}