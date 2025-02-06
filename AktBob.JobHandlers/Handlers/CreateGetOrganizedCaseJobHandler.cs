using AktBob.Shared;
using AktBob.Shared.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using MassTransit.Mediator;
using MassTransit;
using AktBob.Deskpro.Contracts;
using AktBob.Database.Contracts;
using AktBob.Database.UseCases.Tickets.UpdateTicket;

namespace AktBob.JobHandlers.Handlers;
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
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            _logger.LogInformation("Creating GetOrganized Case (requeust from Deskpro ID: {deskproId}", job.DeskproId);

            var caseOwner = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:DefaultCaseOwner"));
            var facet = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:Facet"));
            var caseTypePrefix = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:CaseTypePrefix"));
            var caseStatus = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:CaseStatus"));
            var caseAccess = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("GetOrganized:CaseAccess"));
            var deskproWebhookId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("Deskpro:Webhooks:OpdaterTicketMedGoAktindsigtssag"));


            // 1. Create GetOrganized case
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
            var caseUrl = createCaseResult.Value.CaseUrl;

            _logger.LogInformation("GO case {getOrganizedCaseId} created (Deskpro ID: {deskproId}, GO Case Url: {getOrganizedCaseUrl})", caseId, job.DeskproId, caseUrl);


        
            // 2. Update Deskpro field
            var caseUrlClean = caseUrl.Replace("ad.", "");

            _logger.LogInformation($"Updating Deskpro ticket. Invoking webhook ID {deskproWebhookId}. Payload: getOrganizedCaseId = {caseId}, getOrganizedCaseUrl = {caseUrlClean}, deskproTicketId = {job.DeskproId}");

            var payload = new
            {
                GetOrganizedCaseId = caseId,
                GetOrganizedCaseUrlClean = caseUrlClean,
                DeskproTicketId = job.DeskproId
            };

            var invokeWebhookCommand = new InvokeWebhookCommand(deskproWebhookId, payload);
            await mediator.Send(invokeWebhookCommand, cancellationToken);
                        
            _logger.LogInformation($"webhook ID {deskproWebhookId} invoked. Payload: getOrganizedCaseId = {caseId}, getOrganizedCaseUrl = {caseUrlClean}, deskproTicketId = {job.DeskproId}");



            // 3. Update database
            _logger.LogInformation("Updating database, setting GetOrganized case '{caseId}' for case with DeskproId {deskproId}", caseId, job.DeskproId);

            var getDatabaseTicketQuery = new GetTicketsQuery(job.DeskproId, null, null);
            var getDatabaseTicketResult = await mediator.SendRequest(getDatabaseTicketQuery, cancellationToken);

            if (!getDatabaseTicketResult.IsSuccess || getDatabaseTicketResult.Value is null)
            {
                _logger.LogError("Error getting database ticket for DeskproId {id}", job.DeskproId);
                return;
            }

            var databaseTicket = getDatabaseTicketResult.Value.First();

            var updateDatabaseTicketCommand = new UpdateTicketCommand(databaseTicket.Id, caseId, null, null, null);
            var updateDatabaseTicketResult = await mediator.SendRequest(updateDatabaseTicketCommand, cancellationToken);

            if (!updateDatabaseTicketResult.IsSuccess)
            {
                _logger.LogError("Error updating database ticket ID {id} (DeskproId: {deskproId}) setting GetOrganized CaseId '{caseId}'", databaseTicket.Id, job.DeskproId, caseId);
                return;
            }
        }
    }
}