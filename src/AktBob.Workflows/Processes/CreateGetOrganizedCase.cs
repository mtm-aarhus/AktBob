using AktBob.Shared.Jobs;
using AktBob.GetOrganized.Contracts;
using AktBob.Database.Contracts;
using System.Text.Json;
using Hangfire;
using AktBob.Shared.Extensions;
using Aktbob.Modules.Deskpro.Features.GetTicket;
using Aktbob.Modules.Deskpro.Features.InvokeWebhook;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace AktBob.Workflows.Processes;
internal class CreateGetOrganizedCase(
    IConfiguration configuration,
    ILogger<CreateGetOrganizedCase> logger,
    IServiceScopeFactory serviceScopeFactory)
    : IJobHandler<CreateGetOrganizedCaseJob>
{
    [AutomaticRetry(Attempts = 3)]
    public async Task Handle(CreateGetOrganizedCaseJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(job.TicketId);

        using var scope = serviceScopeFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredServiceOrThrow<IJobDispatcher>();
        var deskproGetTicketHandler = scope.ServiceProvider.GetRequiredServiceOrThrow<IGetTicketHandler>();
        var unitOfWork = scope.ServiceProvider.GetRequiredServiceOrThrow<IUnitOfWork>();
        var getOrganized = scope.ServiceProvider.GetRequiredServiceOrThrow<IGetOrganizedModule>();

        // Get subject from Deskpro
        var deskproTicketResult = await deskproGetTicketHandler.Handle(job.TicketId, cancellationToken);
        if (deskproTicketResult.IsError) throw new BusinessException("Unable to get ticket from Deskpro");

        // Create GO-case
        var caseTitle = deskproTicketResult.Value.Subject ?? "Uden titel";
        var caseProfile = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CreateGetOrganizedCase:CaseProfile"));
        var status = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CreateGetOrganizedCase:CaseStatus"));
        var access = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CreateGetOrganizedCase:CaseAccess"));
        var department = MapDepartment(deskproTicketResult.Value.Fields);
        var facet = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CreateGetOrganizedCase:Facet"));
        var kle = MapKle(deskproTicketResult.Value.Fields);

        var createCaseResult = await getOrganized.CreateCase(caseTitle, caseProfile, status, access, department, facet, kle, cancellationToken: cancellationToken);
        if (createCaseResult.IsError) throw new BusinessException($"{createCaseResult.FirstError.Code}: {createCaseResult.FirstError.Description}");

        var caseId = createCaseResult.Value.CaseId;
        var caseUrl = createCaseResult.Value.CaseUrl.Replace("ad.", "");

        logger.LogInformation("GetOrganized case {caseId} created", caseId);

        UpdateDeskproSetGetOrganizedCaseId(scope, job.TicketId, caseId, caseUrl, cancellationToken);
        await UpdateDatabaseSetGetOrganizedCaseId(job.TicketId, unitOfWork, caseId, caseUrl);
        jobDispatcher.Dispatch(new RegisterMessagesJob(job.TicketId), TimeSpan.FromMinutes(1)); // Add Deskpro messages to the just created GO-case
    }

    // Map Deskpro field "afdeling" to GetOrganized department
    private string MapDepartment(IEnumerable<FieldDto> fields)
    {
        var mapping = configuration.GetSection("CreateGetOrganizedCase:DepartmentMapping").GetChildren().ToDictionary(x => x.Key, x => x.Value);
        var fieldId = configuration.GetValue<int?>("Deskpro:Fields:Afdeling");
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
        var mapping = configuration.GetSection("CreateGetOrganizedCase:KleMapping").GetChildren().ToDictionary(x => x.Key, x => x.Value);
        var fieldId = configuration.GetValue<int?>("Deskpro:Fields:Afdeling");
        var fieldChoices = fields.FirstOrDefault(x => x.Id == fieldId)?.Values ?? [];

        if (!fieldChoices.Any() || mapping.Count() == 0)
        {
            return string.Empty;
        }

        return mapping.Where(m => fieldChoices.Contains(m.Key)).Select(m => m.Value).FirstOrDefault() ?? string.Empty;
    }

    private async Task UpdateDatabaseSetGetOrganizedCaseId(int ticketId, IUnitOfWork unitOfWork, string caseId, string caseUrl)
    {
        var ticket = await unitOfWork.Tickets.GetByDeskproTicketId(ticketId);
        if (ticket is null)
        {
            return;
        }

        ticket.CaseNumber = caseId;
        ticket.CaseUrl = caseUrl;

        await unitOfWork.Tickets.Update(ticket);
    }

    private void UpdateDeskproSetGetOrganizedCaseId(IServiceScope scope, int ticketId, string caseId, string caseUrl, CancellationToken cancellationToken)
    {
        var deskproInvokeWebhookHandler = scope.ServiceProvider.GetRequiredServiceOrThrow<IInvokeWebhookHandler>();
        
        var deskproWebhookId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:Webhooks:UpdateTicketSetGoCaseId"));
        var payload = new
        {
            GetOrganizedCaseId = caseId,
            GetOrganizedCaseUrlClean = caseUrl,
            DeskproTicketId = ticketId
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        deskproInvokeWebhookHandler.Handle(deskproWebhookId, json, cancellationToken);
    }
}