using AktBob.FilArkiv.Contracts;
using AktBob.Podio.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

internal class CheckOCRScreeningStatusRegisterFiles(IServiceScopeFactory serviceScopeFactory, ILogger<CheckOCRScreeningStatusRegisterFiles> logger, IConfiguration configuration) : IJobHandler<CheckOCRScreeningStatusRegisterFilesJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CheckOCRScreeningStatusRegisterFiles> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(CheckOCRScreeningStatusRegisterFilesJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredServiceOrThrow<IJobDispatcher>();
        var podio = scope.ServiceProvider.GetRequiredServiceOrThrow<IPodioModule>();
        var filArkiv = scope.ServiceProvider.GetRequiredServiceOrThrow<IFilArkivModule>();
        var cachedData = CachedData.Instance;

        var @case = new Case(job.FilArkivCaseId, job.PodioItemId);

        if (!cachedData.Cases.TryAdd(job.FilArkivCaseId, @case))
        {
            if (cachedData.Cases.ContainsKey(job.FilArkivCaseId))
            {
                _logger.LogWarning("Case {caseId} already added to cache", job.FilArkivCaseId);
                return;
            }

            throw new BusinessException("Unable to add case to cache");
        }

        var documents = await filArkiv.GetDocumentsByCaseId(@case.FilArkivCaseId, cancellationToken);
        if (!documents.IsSuccess) throw new BusinessException($"Unable to get document for case {@case.FilArkivCaseId} from FilArkiv");
        
        foreach (var document in documents.Value)
        {
            var documentFileIds = document.Files.Select(f => f.Id);
            @case.Files.AddRange(documentFileIds.Select(f => new KeyValuePair<Guid, File>(f, new File())));
        }

        _logger.LogDebug("Case {caseId}: {count} files registered", @case.FilArkivCaseId, @case.Files.Count());

        if (Settings.ShouldUpdatePodioItemImmediately(_configuration))
        {
            jobDispatcher.Dispatch(new UpdatePodioFilArkivFieldsJob(job.PodioItemId, @case.FilArkivCaseId));
        }

        // Enqueue job: query files processing status
        foreach (var file in @case.Files)
        {
            jobDispatcher.Dispatch(new QueryFileProcessingStatusJob(job.PodioItemId, job.FilArkivCaseId, file.Key, 0));
        }
    }
}