using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.JobHandlers.Utils;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Podio.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using AktBob.UiPath.Contracts;
using Ardalis.GuardClauses;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.JobHandlers.Handlers;
internal class CreateDocumentListQueueItemJobHandler(ILogger<CreateDocumentListQueueItemJobHandler> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateDocumentListQueueItemJob>
{
    private readonly ILogger<CreateDocumentListQueueItemJobHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly string _configurationObjectName = "CreateDocumentListQueueItemJobHandler";

    public async Task Handle(CreateDocumentListQueueItemJob job, CancellationToken cancellationToken = default)
    {
        // UiPath variables
        var tenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:UiPathQueueName:{tenancyName}"));

        // OpenOrchestrator variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:OpenOrchestratorQueueName"));
        var useOpenOrchestrator = _configuration.GetValue<bool>($"{_configurationObjectName}:UseOpenOrchestrator");

        // Podio variables
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioField>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "CaseNumber"));

        Guard.Against.Null(podioFieldCaseNumber.Value);

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Get metadata from Podio
            if (!GetCaseNumberFromPodioItem(mediator, podioAppId, job.PodioItemId, podioFieldCaseNumber.Key, cancellationToken, out string caseNumber))
            {
                return;
            }

            // Find ticket in database from PodioItemId
            if (!GetTicketFromApiDatabaseByPodioItemId(mediator, job.PodioItemId, cancellationToken, out Database.Contracts.Dtos.TicketDto? databaseTicketDto))
            {
                return;
            }

            // Get ticket from Deskpro
            if (!GetDeskproTicket(mediator, databaseTicketDto!.DeskproId, cancellationToken, out TicketDto? deskproTicketDto))
            {
                return;
            }

            // Get Deskpro ticket agent, if any (returns (string.Empty, string.Empty) if there is no agent)
            GetDeskproTicketAgent(mediator, deskproTicketDto!.Agent, cancellationToken, out (string Name, string Email) agent);

            if (useOpenOrchestrator)
            {
                // Create OpenOrchestrator queue item
                await CreateOpenOrchestratorQueueItem(mediator, openOrchestratorQueueName, caseNumber, agent, job.PodioItemId, deskproTicketDto, cancellationToken);
            }
            else
            {
                await PostUiPathQueueElement(uiPathQueueName, mediator, job.PodioItemId, caseNumber, deskproTicketDto, agent, cancellationToken);
            }
        }
    }

    private async Task CreateOpenOrchestratorQueueItem(IMediator mediator, string queueName, string caseNumber, (string Name, string Email) agent, long podioItemId, TicketDto deskproTicketDto, CancellationToken stoppingToken)
    {
        var data = new
        {
            SagsNummer = caseNumber,
            agent.Email,
            Navn = agent.Name,
            PodioID = podioItemId,
            DeskproID = deskproTicketDto.Id,
            Titel = deskproTicketDto.Subject
        };

        var command = new CreateQueueItemCommand(queueName, data, podioItemId.ToString());
        await mediator.Send(command, stoppingToken);
    }

    private static async Task PostUiPathQueueElement(string uiPathQueueName, IMediator mediator, long podioItemId, string caseNumber, TicketDto deskproTicketDto, (string Name, string Email) agent, CancellationToken stoppingToken)
    {
        var uiPathQueueItemContent = new
        {
            SagsNummer = caseNumber,
            agent.Email,
            Navn = agent.Name,
            PodioID = podioItemId,
            DeskproID = deskproTicketDto.Id,
            Titel = deskproTicketDto.Subject
        };

        var addUiPathQueueItemCommand = new AddQueueItemCommand(uiPathQueueName, podioItemId.ToString(), uiPathQueueItemContent);
        await mediator.Send(addUiPathQueueItemCommand, stoppingToken);
    }

    private bool GetCaseNumberFromPodioItem(IMediator mediator, int podioAppId, long podioItemId, int podioFieldId, CancellationToken cancellationToken, out string caseNumber)
    {
        caseNumber = string.Empty;

        var getPodioItemQuery = new GetItemQuery(podioAppId, podioItemId);
        var getPodioItemQueryResult = mediator.SendRequest(getPodioItemQuery, cancellationToken).GetAwaiter().GetResult();

        if (!getPodioItemQueryResult.IsSuccess)
        {
            _logger.LogError("Could not get item {itemId} from Podio", podioItemId);
            return false;
        }

        caseNumber = getPodioItemQueryResult.Value.GetField(podioFieldId)?.GetValues<FieldValueText>()?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogError("Could not get case number field value from Podio Item {itemId} fieldId {fieldId}", podioItemId, podioFieldId);
            return false;
        }

        return true;
    }

    private bool GetTicketFromApiDatabaseByPodioItemId(IMediator mediator, long podioItemId, CancellationToken cancellationToken, out Database.Contracts.Dtos.TicketDto? ticketDto)
    {
        var retriesCount = 10;
        var counter = 1;
        var delay = TimeSpan.FromSeconds(5);
        ticketDto = null;

        var getTicketByPodioItemIdQuery = new GetTicketsQuery(null, podioItemId, null);

        // Try get some data from the database API based on the provided podioItemId
        // This might fail initially since Podio triggers both the create event and DocumentListTrigger event almost at the same time
        // Because of this, we utilize a simple retry feature. To be absolutely sure, we allow a retry of max of 10. If nothing is found
        // after 10 retries something else is wrong.
        while (counter <= retriesCount || !cancellationToken.IsCancellationRequested)
        {
            var getTicketByPodioItemIdQueryResult = mediator.SendRequest(getTicketByPodioItemIdQuery, cancellationToken).GetAwaiter().GetResult();

            // We have data: Exit the while loop
            if (getTicketByPodioItemIdQueryResult.IsSuccess)
            {
                if (getTicketByPodioItemIdQueryResult.Value.Count() > 1)
                {
                    _logger.LogWarning("{count} Deskpro tickets found for PodioItemId {podioItemId}. Only processing the first.", getTicketByPodioItemIdQueryResult.Value.Count(), podioItemId);
                }

                if (getTicketByPodioItemIdQueryResult.Value.Count() > 0)
                {
                    _logger.LogInformation("Try {count}/{retries}: Database API ticket data for PodioItemId {id} found", counter, retriesCount, podioItemId);
                    ticketDto = getTicketByPodioItemIdQueryResult.Value.First();
                    break;
                }
            }

            // No data was found: retry
            _logger.LogWarning("Try {count}/{retries}: Database API did not return any ticket data for PodioItemId {id}. Retry in {time} ...", counter, retriesCount, podioItemId, delay.ToString());

            counter++;
            Task.Delay(delay).GetAwaiter().GetResult();
        }

        if (ticketDto == null)
        {
            _logger.LogError("Final try: Database API did not return any ticket data for PodioItemId {id}", podioItemId);
            return false;
        }

        return true;
    }

    private bool GetDeskproTicket(IMediator mediator, int deskproId, CancellationToken cancellationToken, out TicketDto? ticketDto)
    {
        ticketDto = null;

        var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(deskproId);
        var getDeskproTicketQueryResult = mediator.SendRequest(getDeskproTicketQuery, cancellationToken).GetAwaiter().GetResult();

        if (!getDeskproTicketQueryResult.IsSuccess)
        {
            _logger.LogError("Deskpro ticket #{id} not found in Deskpro", deskproId);
            return false;
        }

        ticketDto = getDeskproTicketQueryResult.Value;
        return true;
    }

    private void GetDeskproTicketAgent(IMediator mediator, PersonDto? person, CancellationToken cancellationToken, out (string AgentName, string AgentEmail) agent)
    {
        agent = (string.Empty, string.Empty);

        if (person is null)
        {
            _logger.LogWarning("Person object is null");
            return;
        }

        if (person.Id <= 0)
        {
            _logger.LogWarning("Agent Id is zero");
            return;
        }

        var getAgentQuery = new GetDeskproPersonQuery(person.Id);
        var getAgentResult = mediator.SendRequest(getAgentQuery, cancellationToken).GetAwaiter().GetResult();

        if (getAgentResult.IsSuccess
            && getAgentResult.Value is not null
            && getAgentResult.Value.IsAgent)
        {
            agent = (getAgentResult.Value.FullName, getAgentResult.Value.Email);
            return;
        }

        _logger.LogWarning("Deskpro agent not found for agentId #{id}", person.Id);
        return;
    }
}
