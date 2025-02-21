using AktBob.Shared.Contracts;
using AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;

namespace AktBob.JobHandlers.Handlers.CreateGetOrganizedCase;
internal class CreateGetOrganizedCaseJobHandler : IJobHandler<CreateGetOrganizedCaseJob>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreateGetOrganizedCaseJobHandler> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CreateGetOrganizedCaseJobHandler(
        IConfiguration configuration,
        ILogger<CreateGetOrganizedCaseJobHandler> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Handle(CreateGetOrganizedCaseJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        _logger.LogInformation("Creating GetOrganized case (Deskpro ID {deskproId}", job.DeskproId);

        var caseOwner = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:DefaultCaseOwner"));
        var facet = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:Facet"));
        var caseTypePrefix = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:CaseTypePrefix"));
        var caseStatus = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:CaseStatus"));
        var caseAccess = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:CaseAccess"));

        var createCaseCommand = new GetOrganized.Contracts.CreateCaseCommand(
            CaseTypePrefix: caseTypePrefix,
            CaseTitle: job.CaseTitle,
            Description: string.Empty,
            Status: caseStatus,
            Access: caseAccess);

        var createCaseResult = await mediator.SendRequest(createCaseCommand, cancellationToken);

        if (!createCaseResult.IsSuccess)
        {
            _logger.LogError("Error creating GetOrganized case (DeskproId {deskproId}", job.DeskproId);
            return;
        }

        var caseId = createCaseResult.Value.CaseId;
        var caseUrl = createCaseResult.Value.CaseUrl.Replace("ad.", "");

        _logger.LogInformation("GO case {getOrganizedCaseId} created (Deskpro ID: {deskproId}, GO Case Url: {getOrganizedCaseUrl})", caseId, job.DeskproId, caseUrl);

        BackgroundJob.Enqueue<UpdateDeskproField>(x => x.SetGetOrganizedCaseId(job.DeskproId, caseId, caseUrl, CancellationToken.None));
        BackgroundJob.Enqueue<UpdateDatabase>(x => x.SetGetOrganizedCaseId(job.DeskproId, caseId, caseUrl, CancellationToken.None));
        BackgroundJob.Schedule<RegisterMessagesJobHandler>(x => x.Handle(new RegisterMessagesJob(job.DeskproId), CancellationToken.None), TimeSpan.FromMinutes(2));
    }
}