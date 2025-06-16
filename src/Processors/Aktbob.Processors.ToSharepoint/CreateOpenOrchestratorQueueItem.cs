using System.Text.Json;
using System.Text.Json.Nodes;
using AktBob.Database.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Exceptions;
using AktBob.Shared.Extensions;
using AktBob.Shared.ModuleClients.DeskproModule;
using AktBob.Shared.ModuleClients.OpenOrchestratorModule;
using AktBob.Shared.ModuleClients.PodioModule;
using AktBob.Shared.Processors;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;
using ErrorOr;
using Microsoft.Extensions.Configuration;

namespace Aktbob.Processors.ToSharepoint;

public class CreateOpenOrchestratorQueueItem(
    ILogger<CreateOpenOrchestratorQueueItem> logger,
    IConfiguration configuration,
    IOpenOrchestratorModuleClient openOrchestrator,
    IDeskproModuleClient deskpro,
    IPodioModuleClient podio,
    IUnitOfWork unitOfWork)
{
    [Function(nameof(CreateOpenOrchestratorQueueItem))]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body: {body}", message.Body);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        // Deserialize message body to expected job type
        var job = JsonSerializer.Deserialize<ToSharepointJob>(message.Body, SerializerConfiguration.SerializerOptions());
        if (job is null)
        {
            throw new BusinessException($"{LogSnippets.MessageDeliveryCount(message.MessageId, message.DeliveryCount)}: Body could not be deserialized to type {nameof(ToSharepointJob)}. Body content = {message.Body}");
        }
        
        Guard.Against.NegativeOrZero(job.PodioItemId);

        // Variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("OpenOrchestratorQueueName"));
        var podioAppId = Guard.Against.Null(configuration.GetValue<int?>("Podio:AppId"));
        var podioCaseNumberFieldId = Guard.Against.Null(configuration.GetValue<int?>("Podio:CaseNumberFieldId"));

        // Get data
        var getPodioItem = podio.GetItem(podioAppId, job.PodioItemId, cancellationToken);
        var getDatabaseCases = unitOfWork.Cases.GetAll(job.PodioItemId, null);
        var getDatabaseTicket = unitOfWork.Tickets.GetByPodioItemId(job.PodioItemId);

        await Task.WhenAll([
            getPodioItem,
            getDatabaseCases,
            getDatabaseTicket]);

        if (getPodioItem.Result.IsError) throw new BusinessException(getPodioItem.Result.Errors.ToCommaDelimitedString());
        if (getDatabaseCases.Result.FirstOrDefault() is null) throw new BusinessException("Unable to get case from databaase");
        if (getDatabaseTicket.Result is null) throw new BusinessException("Unable to get ticket from database");

        var databaseCase = getDatabaseCases.Result.First();

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
            caseNumber = databaseCase.CaseNumber ?? string.Empty;

            if (string.IsNullOrEmpty(caseNumber))
            {
                logger.LogWarning("Unable to get case number from Database");
            }
        }

        if (string.IsNullOrEmpty(databaseCase.SharepointFolderName)) logger.LogWarning("Case related to Podio item {id}: SharepointFolderName is null or empty", job.PodioItemId);
        
        var filArkivCaseId = getDatabaseTicket.Result.Cases?.FirstOrDefault(c => c.PodioItemId == job.PodioItemId)?.FilArkivCaseId;
        if (filArkivCaseId == null)
        {
            logger.LogWarning("FilArkivCaseId not found for Podio item {podioItemId}", job.PodioItemId);
        }

        // Get ticket from Deskpro
        var deskproTicketResult = await deskpro.GetTicket(getDatabaseTicket.Result.DeskproId, cancellationToken);
        if (deskproTicketResult.IsError) throw new BusinessException("Unable to get ticket from Deskpro");

        // Get Deskpro agent
        var agent = deskproTicketResult.Value.Agent?.Id != null
            ? await deskpro.GetPersonById(deskproTicketResult.Value.Agent.Id, cancellationToken)
            : Error.NotFound().ToErrorOr<PersonDto>();

        // Create queue item
        var payload = new
        {
            Sagsnummer = caseNumber,
            MailModtager = agent.Value.Email,
            DeskProID = deskproTicketResult.Value.Id,
            DeskProTitel = deskproTicketResult.Value.Subject,
            PodioID = job.PodioItemId,
            Overmappe = getDatabaseTicket.Result.SharepointFolderName,
            Undermappe = databaseCase.SharepointFolderName,
            GeoSag = !caseNumber.IsNovaCase(),
            NovaSag = caseNumber.IsNovaCase(),
            AktSagsURL = getDatabaseTicket.Result.CaseUrl,
            FilarkivCaseID = databaseCase.FilArkivCaseId
        };

        await openOrchestrator.AddQueueItem(openOrchestratorQueueName, $"Podio {job.PodioItemId.ToString()}", payload, cancellationToken);
    }
}