﻿using AktBob.Shared.Jobs;
using AktBob.GetOrganized.Contracts;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using System.Text.Json;
using Hangfire;
using AktBob.Shared.Extensions;

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

    [AutomaticRetry(Attempts = 3)]
    public async Task Handle(CreateGetOrganizedCaseJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(job.DeskproId);

        using var scope = _serviceScopeFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredServiceOrThrow<IJobDispatcher>();
        var deskpro = scope.ServiceProvider.GetRequiredServiceOrThrow<IDeskproModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredServiceOrThrow<IUnitOfWork>();
        var getOrganized = scope.ServiceProvider.GetRequiredServiceOrThrow<IGetOrganizedModule>();

        // Get subject from Deskpro
        var deskproTicketResult = await deskpro.GetTicket(job.DeskproId, cancellationToken);
        if (!deskproTicketResult.IsSuccess) throw new BusinessException("Unable to get ticket from Deskpro");

        // Create GO-case
        var caseTitle = deskproTicketResult.Value.Subject ?? "Uden titel";
        var caseProfile = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateGetOrganizedCase:CaseProfile"));
        var status = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateGetOrganizedCase:CaseStatus"));
        var access = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateGetOrganizedCase:CaseAccess"));
        var department = MapDepartment(deskproTicketResult.Value.Fields);
        var facet = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateGetOrganizedCase:Facet"));
        var kle = MapKle(deskproTicketResult.Value.Fields);

        var createCaseCommand = new CreateGetOrganizedCaseCommand(caseTitle, caseProfile, status, access, department, facet, kle);
        var createCaseResult = await getOrganized.CreateCase(createCaseCommand, cancellationToken: cancellationToken);

        if (!createCaseResult.IsSuccess) throw new BusinessException("Unable to create GetOrganized case");

        var caseId = createCaseResult.Value.CaseId;
        var caseUrl = createCaseResult.Value.CaseUrl.Replace("ad.", "");

        _logger.LogInformation("GetOrganized case {caseId} created", caseId);

        UpdateDeskproSetGetOrganizedCaseId(deskpro, job.DeskproId, caseId, caseUrl);
        await UpdateDatabaseSetGetOrganizedCaseId(job.DeskproId, unitOfWork, caseId, caseUrl);
        jobDispatcher.Dispatch(new RegisterMessagesJob(job.DeskproId), TimeSpan.FromMinutes(1)); // Add Deskpro messages to the just created GO-case
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

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        deskproModule.InvokeWebhook(deskproWebhookId, json);
    }
}