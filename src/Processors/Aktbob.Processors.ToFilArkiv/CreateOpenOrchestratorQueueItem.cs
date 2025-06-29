using System.Text.Json.Nodes;
using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Contracts.Modules.Podio;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using AktBob.Shared.ModuleClients.DeskproModule;
using AktBob.Shared.ModuleClients.OpenOrchestratorModule;
using AktBob.Shared.ModuleClients.PodioModule;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;
using ErrorOr;
using Microsoft.Extensions.Configuration;

namespace Aktbob.Processors.ToFilArkiv;

public class CreateOpenOrchestratorQueueItem(
    IConfiguration configuration,
    ILogger<CreateOpenOrchestratorQueueItem> logger,
    IOpenOrchestratorModuleClient openOrchestrator,
    IDeskproModuleClient deskpro,
    IPodioModuleClient podio,
    IUnitOfWork unitOfWork)
{
    [Function("create-open-orchestrator-queue-item")]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        var job = MessageDeserializer.Deserialize<ToFilArkivJob>(message);
        
        // Get variables from configuration
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("OpenOrchestratorQueueName"));
        var podioAppId = Guard.Against.Null(configuration.GetValue<int?>("Podio:AppId"));
        var podioCaseNumberFieldId = Guard.Against.Null(configuration.GetValue<int?>("Podio:CaseNumberFieldId"));

        // Get data
        var getPodioItem = podio.GetItem(podioAppId, job.PodioItemId, cancellationToken);
        var getDatabaseCase = unitOfWork.Cases.GetAll(job.PodioItemId, null);
        var getDatabaseTicket = unitOfWork.Tickets.GetByPodioItemId(job.PodioItemId);

        await Task.WhenAll([
            getPodioItem,
            getDatabaseCase,
            getDatabaseTicket]);

        
        // Ensure external data requests was successful
        var dataRequestsSuccess = EnsureExternalRequests(getPodioItem.Result, getDatabaseCase.Result, getDatabaseTicket.Result);
        if (!dataRequestsSuccess.IsSuccess)
        {
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: dataRequestsSuccess.DeadLetterReason, deadLetterErrorDescription: dataRequestsSuccess.DeadLetterDescription, cancellationToken: cancellationToken);
            return;
        }
        
        // Get case number value from Podio module response
        var caseNumber = string.Empty;
        var caseNumberFieldValue = JsonValue.Create(getPodioItem.Result.Value.Fields.FirstOrDefault(x => x.Id == podioCaseNumberFieldId)?.Value);
        
        if (caseNumberFieldValue is JsonNode and JsonValue valueNode)
        {
            if (valueNode.TryGetValue<string>(out var value))
            {
                caseNumber = value;
            }
        }

        // Case number was not found on the Podio item -> fallback: assign case number from database
        if (string.IsNullOrEmpty(caseNumber))
        {
            logger.LogWarning("Unable to get case number field value from Podio Item {itemId}", job.PodioItemId);
            caseNumber = getDatabaseCase.Result.FirstOrDefault()?.CaseNumber ?? string.Empty;

            if (string.IsNullOrEmpty(caseNumber))
            {
                logger.LogWarning("Unable to get case number from Database");
            }
        }

        // Get data from Deskpro
        var deskproTicketResult = await deskpro.GetTicket(getDatabaseTicket.Result.DeskproId, cancellationToken);
        if (deskproTicketResult.IsError)
        {
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"Unable to get ticket {getDatabaseTicket.Result.DeskproId} from Deskpro", cancellationToken: cancellationToken);
            return;
        }

        // Get Deskpro agent
        var agent = deskproTicketResult.Value.Agent?.Id != null
            ? await deskpro.GetPersonById(deskproTicketResult.Value.Agent.Id, cancellationToken)
            : Error.NotFound().ToErrorOr<PersonDto>();

        // Get Deskpro person
        var person = deskproTicketResult.Value.Person?.Id != null
            ? await deskpro.GetPersonById(deskproTicketResult.Value.Person.Id, cancellationToken)
            : Error.NotFound().ToErrorOr<PersonDto>();

        // Create OpenOrchestrrator queue item
        var isNovaCase = caseNumber.IsNovaCase();
        
        var payload = new
        {
            Sagsnummer = caseNumber,
            MailModtager = agent.Value.Email,
            DeskProID = getDatabaseTicket.Result.DeskproId,
            DeskProTitel = deskproTicketResult.Value.Subject,
            PodioID = job.PodioItemId,
            Overmappe = getDatabaseTicket.Result.SharepointFolderName,
            Undermappe = getDatabaseCase.Result.First().SharepointFolderName,
            GeoSag = !isNovaCase,
            NovaSag = isNovaCase,
            AktSagsURL = getDatabaseTicket.Result.CaseUrl,
            IndsenderNavn = person.Value.FullName,
            IndsenderMail = person.Value.Email,
            AktindsigtsDato = deskproTicketResult.Value.CreatedAt
        };

        await openOrchestrator.AddQueueItem(openOrchestratorQueueName, $"Podio {job.PodioItemId.ToString()}", payload, cancellationToken);
    }

    private static (bool IsSuccess, string DeadLetterReason, string DeadLetterDescription) EnsureExternalRequests(
        ErrorOr<ItemDto> getPodioItem,
        IReadOnlyCollection<Case> getDatabaseCase,
        Ticket? getDatabaseTicket)
    {
        if (getPodioItem.IsError) return (false, $"Error getting item from Podio.", getPodioItem.Errors.ToCommaDelimitedString());
        if (getDatabaseCase.FirstOrDefault() is null) return (false, $"Unable to get case by PodioItemId from database", string.Empty);
        if (getDatabaseTicket is null) return (false, $"Unable to get ticket by PodioItemId from database", string.Empty);
        if (string.IsNullOrEmpty(getDatabaseTicket.SharepointFolderName)) return (false, $"SharepointFolderName is null or empty", string.Empty);
        return (true, string.Empty, string.Empty);
    }
}