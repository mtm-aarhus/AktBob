using AAK.Podio;
using AktBob.DatabaseAPI.Contracts.Queries;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Podio.Contracts;
using AktBob.Queue.Contracts;
using AktBob.UiPath.Contracts;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AktBob.PodioHookProcessor.UseCases.DocumentListTrigger;
internal class BackgroundWorker : BackgroundService
{
    private ILogger<BackgroundWorker> _logger;
    private IConfiguration _configuration;
    private readonly IPodio _podio;

    public IServiceProvider ServiceProvider { get; }

    public BackgroundWorker(ILogger<BackgroundWorker> logger, IConfiguration configuration, IServiceProvider serviceProvider, IPodio podio)
    {
        _logger = logger;
        _configuration = configuration;
        ServiceProvider = serviceProvider;
        _podio = podio;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var azureQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"DocumentListTrigger:AzureQueueName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"DocumentListTrigger:UiPathQueueName:{tenancyName}"));
        var delay = _configuration.GetValue<int?>("DocumentListTrigger:WorkerIntervalSeconds") ?? 10;
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => long.Parse(x.Key), x => x.Get<PodioField>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldCaseNumber.Value);

        using (var scope = ServiceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var messages = await GetQueueMessages(mediator, azureQueueName!, stoppingToken);

                foreach (var message in messages)
                {
                    var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(azureQueueName, message.Id, message.PopReceipt);
                    await mediator.Send(deleteAzureQueueItemCommand);

                    // Map the message body to a Podio Item Id
                    if (!GetPodioItemIdFromQueueMessage(message, out long? podioItemId))
                    {
                        continue;
                    }

                    // Get metadata from Podio
                    if (!GetCaseNumberFromPodioItem(mediator, podioAppId, (long)podioItemId!, podioFieldCaseNumber.Key, stoppingToken, out string caseNumber))
                    {
                        continue;
                    }

                    // Find ticket in database from PodioItemId
                    if (!GetTicketFromApiDatabaseByPodioItemId(mediator, (long)podioItemId!, stoppingToken, out DatabaseAPI.Contracts.DTOs.TicketDto? databaseTicketDto))
                    {
                        continue;
                    }

                    // Get ticket from Deskpro
                    if (!GetDeskproTicket(mediator, databaseTicketDto!.DeskproId, stoppingToken, out TicketDto? deskproTicketDto))
                    {
                        continue;
                    }

                    // Get Deskpro ticket agent, if any (returns (string.Empty, string.Empty) if there is no agent)
                    GetDeskproTicketAgent(mediator, deskproTicketDto!.Agent, stoppingToken, out (string Name, string Email) agent);

                    // UiPath queue element
                    await PostUiPathQueueElement(uiPathQueueName, mediator, (long)podioItemId, caseNumber, deskproTicketDto, agent, stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
        }
    }

    private static async Task PostUiPathQueueElement(string uiPathQueueName, IMediator mediator, long podioItemId, string caseNumber, TicketDto deskproTicketDto, (string Name, string Email) agent, CancellationToken stoppingToken)
    {
        var uiPathQueueItemContent = new
        {
            SagsNummer = caseNumber,
            Email = agent.Email,
            Navn = agent.Name,
            PodioID = podioItemId,
            DeskproID = deskproTicketDto.Id,
            Titel = deskproTicketDto.Subject
        };

        // Post UiPath queue item
        var addUiPathQueueItemCommand = new AddQueueItemCommand(uiPathQueueName, podioItemId.ToString(), uiPathQueueItemContent);
        await mediator.Send(addUiPathQueueItemCommand, stoppingToken);
    }

    private bool GetPodioItemIdFromQueueMessage(QueueMessageDto message, out long? id)
    {
        id = null;

        if (string.IsNullOrEmpty(message.Body))
        {
            _logger.LogError("Azure queue item body is empty");
            return false;
        }

        string base64EncodedMessage = message.Body.ToString();
        string podioItemIdString = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedMessage));

        if (!long.TryParse(podioItemIdString, out long podioItemId))
        {
            _logger.LogError("Could not parse the string '{string}' as a Podio Item Id", podioItemIdString);
            return false;
        }

        id = podioItemId;
        return true;
    }

    private async Task<IEnumerable<QueueMessageDto>> GetQueueMessages(IMediator mediator, string queueName, CancellationToken cancellationToken)
    {
        var getQueueMessagesQuery = new GetQueueMessagesQuery(queueName);
        var getQueueMessagesResult = await mediator.Send(getQueueMessagesQuery, cancellationToken);

        if (!getQueueMessagesResult.IsSuccess)
        {
            return Enumerable.Empty<QueueMessageDto>();
        }

        return getQueueMessagesResult.Value;
    }

    private bool GetCaseNumberFromPodioItem(IMediator mediator, int podioAppId, long podioItemId, long podioFieldId, CancellationToken cancellationToken, out string caseNumber)
    {
        caseNumber = string.Empty;

        var getPodioItemQuery = new GetItemQuery(podioAppId, podioItemId);
        var getPodioItemQueryResult = mediator.Send(getPodioItemQuery, cancellationToken).GetAwaiter().GetResult();

        if (!getPodioItemQueryResult.IsSuccess)
        {
            _logger.LogError("Could not get item {itemId} from Podio", podioItemId);
            return false;
        }

        caseNumber = getPodioItemQueryResult.Value.Fields.FirstOrDefault(x => x.Id == podioFieldId)?.Value?.FirstOrDefault() ?? string.Empty;

        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogError("Could not get case number field value from Podio Item {itemId} fieldId {fieldId}", podioItemId, podioFieldId);
            return false;
        }

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
        var getAgentResult = mediator.Send(getAgentQuery, cancellationToken).GetAwaiter().GetResult();

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

    private bool GetDeskproTicket(IMediator mediator, int deskproId, CancellationToken cancellationToken, out TicketDto? ticketDto)
    {
        ticketDto = null;

        var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(deskproId);
        var getDeskproTicketQueryResult = mediator.Send(getDeskproTicketQuery, cancellationToken).GetAwaiter().GetResult();

        if (!getDeskproTicketQueryResult.IsSuccess)
        {
            _logger.LogError("Deskpro ticket #{id} not found in Deskpro", deskproId);
            return false;
        }

        ticketDto = getDeskproTicketQueryResult.Value;
        return true;
    }

    private bool GetTicketFromApiDatabaseByPodioItemId(IMediator mediator, long podioItemId, CancellationToken cancellationToken, out DatabaseAPI.Contracts.DTOs.TicketDto? ticketDto)
    {
        var retriesCount = 10;
        var counter = 1;
        var delay = TimeSpan.FromSeconds(5);
        ticketDto = null;

        var getTicketByPodioItemIdQuery = new GetTicketByPodioItemIdQuery(podioItemId);

        // Try get some data from the database API based on the provided podioItemId
        // This might fail initially since Podio triggers both the create event and DocumentListTrigger event almost at the same time
        // Because of this, we utilize a simple retry feature. To be absolutely sure, we allow a retry of max of 10. If nothing is found
        // after 10 retries something else is wrong.
        while (counter <= retriesCount || !cancellationToken.IsCancellationRequested)
        {
            var getTicketByPodioItemIdQueryResult = mediator.Send(getTicketByPodioItemIdQuery, cancellationToken).GetAwaiter().GetResult();

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
}
