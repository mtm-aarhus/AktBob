using AktBob.Shared.Jobs;
using AktBob.GetOrganized.Contracts;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;

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
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var getOrganized = scope.ServiceProvider.GetRequiredService<IGetOrganizedModule>();

        _logger.LogInformation("Creating GetOrganized case (Deskpro ID {deskproId})", job.DeskproId);


        // Get subject from Deskpro
        var deskproTicketResult = await deskpro.GetTicket(job.DeskproId, cancellationToken);
        if (!deskproTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting Deskpro ticket {id}", job.DeskproId);
            return;
        }

        // Create GO-case
        var createCaseResult = await getOrganized.CreateCase(
            caseTitle: deskproTicketResult.Value.Subject ?? "Uden titel",
            caseProfile: Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateGetOrganizedCase:CaseProfile")),
            status: Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateGetOrganizedCase:CaseStatus")),
            access: Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateGetOrganizedCase:CaseAccess")),
            department: MapDepartment(deskproTicketResult.Value.Fields),
            facet: Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateGetOrganizedCase:Facet")),
            kle: MapKle(deskproTicketResult.Value.Fields),
            cancellationToken: cancellationToken);

        if (!createCaseResult.IsSuccess)
        {
            _logger.LogError("Error creating GetOrganized case (DeskproId {deskproId}", job.DeskproId);
            return;
        }

        var caseId = createCaseResult.Value.CaseId;
        var caseUrl = createCaseResult.Value.CaseUrl.Replace("ad.", "");

        _logger.LogInformation("GO case {getOrganizedCaseId} created (Deskpro ID: {deskproId}, GO Case Url: {getOrganizedCaseUrl})", caseId, job.DeskproId, caseUrl);

        UpdateDeskproSetGetOrganizedCaseId(deskpro, job.DeskproId, caseId, caseUrl);
        await UpdateDatabaseSetGetOrganizedCaseId(job.DeskproId, unitOfWork, caseId, caseUrl);
        jobDispatcher.Dispatch(new RegisterMessagesJob(job.DeskproId), TimeSpan.FromMinutes(2)); // Add Deskpro messages to the just created GO-case
    }

    // Map Deskpro field "afdeling" to GetOrganized department
    private string MapDepartment(IEnumerable<FieldDto> fields)
    {
        var mapping = _configuration.GetSection("CreateGetOrganizedCase:DepartmentMapping").GetChildren().ToDictionary(x => x.Key, x => x.Value);
        var fieldId = _configuration.GetValue<int?>("Deskpro:Fields:Afdeling");
        var fieldChoices = fields.FirstOrDefault(x => x.Id == fieldId)?.Values ?? [];

        if (!fieldChoices.Any() || mapping.Count() == 0)
        {
            return string.Empty;
        }

        return mapping.Where(m => fieldChoices.Contains(m.Key)).Select(m => m.Value).FirstOrDefault() ?? string.Empty;
    }

    // Determine from Deskpro field "afdeling" if we can set the KLE
    private string MapKle(IEnumerable<FieldDto> fields)
    {
        var mapping = _configuration.GetSection("CreateGetOrganizedCase:KleMapping").GetChildren().ToDictionary(x => x.Key, x => x.Value);
        var fieldId = _configuration.GetValue<int?>("Deskpro:Fields:Afdeling");
        var fieldChoices = fields.FirstOrDefault(x => x.Id == fieldId)?.Values ?? [];

        if (!fieldChoices.Any() || mapping.Count() == 0)
        {
            return string.Empty;
        }

        return mapping.Where(m => fieldChoices.Contains(m.Key)).Select(m => m.Value).FirstOrDefault() ?? string.Empty;
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

    private void UpdateDeskproSetGetOrganizedCaseId(IDeskproModule deskproModule, int deskproId, string caseId, string caseUrl)
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