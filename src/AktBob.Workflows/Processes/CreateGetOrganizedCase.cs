using AktBob.Shared.Jobs;
using AktBob.GetOrganized.Contracts;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;

namespace AktBob.Workflows.Processes;
internal class CreateGetOrganizedCase : IJobHandler<CreateGetOrganizedCaseJob>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreateGetOrganizedCase> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CreateGetOrganizedCase(
        IConfiguration configuration,
        ILogger<CreateGetOrganizedCase> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Handle(CreateGetOrganizedCaseJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();
        var deskproModule = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var getOrganized = scope.ServiceProvider.GetRequiredService<IGetOrganizedModule>();

        _logger.LogInformation("Creating GetOrganized case (Deskpro ID {deskproId})", job.DeskproId);

        var caseOwner = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:DefaultCaseOwner"));
        var facet = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:Facet"));
        var caseTypePrefix = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:CaseTypePrefix"));
        var caseStatus = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:CaseStatus"));
        var caseAccess = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:CaseAccess"));


        // Create GO-case
        var createCaseResult = await getOrganized.CreateCase(
            caseTypePrefix: caseTypePrefix,
            caseTitle: job.CaseTitle,
            description: string.Empty,
            status: caseStatus,
            access: caseAccess,
            cancellationToken: cancellationToken);

        if (!createCaseResult.IsSuccess)
        {
            _logger.LogError("Error creating GetOrganized case (DeskproId {deskproId}", job.DeskproId);
            return;
        }

        var caseId = createCaseResult.Value.CaseId;
        var caseUrl = createCaseResult.Value.CaseUrl.Replace("ad.", "");

        _logger.LogInformation("GO case {getOrganizedCaseId} created (Deskpro ID: {deskproId}, GO Case Url: {getOrganizedCaseUrl})", caseId, job.DeskproId, caseUrl);

        UpdatePodioSetGetOrganizedCaseId(deskproModule, job.DeskproId, caseId, caseUrl);
        await UpdateDatabaseSetGetOrganizedCaseId(job.DeskproId, unitOfWork, caseId, caseUrl);
        jobDispatcher.Dispatch(new RegisterMessagesJob(job.DeskproId), TimeSpan.FromMinutes(2)); // Add Deskpro messages to the just created GO-case
    }

    private async Task UpdateDatabaseSetGetOrganizedCaseId(int deskproId, IUnitOfWork unitOfWork, string caseId, string caseUrl)
    {
        var ticket = await unitOfWork.Tickets.GetByDeskproTicketId(deskproId);
        if (ticket is null)
        {
            _logger.LogError("Error getting database ticket for DeskproId {id}", deskproId);
            return;
        }

        ticket.CaseNumber = caseId;
        ticket.CaseUrl = caseUrl;

        await unitOfWork.Tickets.Update(ticket);
    }

    private void UpdatePodioSetGetOrganizedCaseId(IDeskproModule deskproModule, int deskproId, string caseId, string caseUrl)
    {
        var deskproWebhookId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("Deskpro:Webhooks:UpdateTicketSetGoCaseId"));
        var payload = new
        {
            GetOrganizedCaseId = caseId,
            GetOrganizedCaseUrlClean = caseUrl,
            DeskproTicketId = deskproId
        };

        deskproModule.InvokeWebhook(deskproWebhookId, payload);
    }
}