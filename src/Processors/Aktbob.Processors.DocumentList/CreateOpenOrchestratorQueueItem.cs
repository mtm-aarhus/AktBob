using AktBob.Database.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Exceptions;
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

namespace Aktbob.Processors.DocumentList;

public class CreateOpenOrchestratorQueueItem(
    ILogger<CreateOpenOrchestratorQueueItem> logger,
    IConfiguration configuration,
    IOpenOrchestratorModuleClient openOrchestrator,
    IPodioModuleClient podio,
    IDeskproModuleClient deskpro,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus)
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

        var job = MessageDeserializer.Deserialize<CreateDocumentListJob>(message);
        Guard.Against.NegativeOrZero(job.PodioItemId);

        // Variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("OpenOrchestratorQueueName"));
        var podioAppId = Guard.Against.Null(configuration.GetValue<int?>("Podio:AppId"));
        var podioCaseNumberFieldId = Guard.Against.Null(configuration.GetValue<int?>("Podio:CaseNumberFieldId")); 

        // Get data
        var getPodioItem = podio.GetItem(podioAppId, job.PodioItemId, cancellationToken);
        var getDatabaseTicket = unitOfWork.Tickets.GetByPodioItemId(job.PodioItemId);

        await Task.WhenAll([getPodioItem, getDatabaseTicket]);

        if (getPodioItem.Result.IsError) throw new BusinessException(getPodioItem.Result.Errors.ToCommaDelimitedString());

        /*
            RETRY: Trying to get the ticket data from the database might fail initially since Podio triggers both the create event
            and DocumentListTrigger event practically at the same time. Because of this race condition, the database might not have
            the ticket data at this point in time yet.

            We handle this by rescheduling this specific job. However, we do not want to reschedule forever, so after 3 retries we stop the rescheduling and exit with an error.

            The retry is scheduled with an exponential delay of 3 seconds raised to the power of the count. So 4 retries = 3s + 9s + 27s + 81s = 120s = 2 minutes.
            If the database still haven't got the ticket data after 2 minutes something else is wrong and there is no need to keep rescheduling. 
        */
        if (getDatabaseTicket.Result is null)
        {
            var count = job.RescheduleCounter + 1;
            if (count > 3) throw new BusinessException($"Reached maximum retries for getting ticket data from database.");
            
            logger.LogWarning("Scheduling retry (count: {count}): ticket data for PodioItem {podioItem} not found in database", count, job.PodioItemId);
                
            // Reschedule
            var serviceBusQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("ServiceBusQueueName"));
            await messageBus.ScheduleMessage(serviceBusQueueName, job with { RescheduleCounter = count }, DateTimeOffset.UtcNow.AddSeconds(Math.Pow(3, count + 1)), cancellationToken);
            return;
        }
        
        var deskproTicket = await deskpro.GetTicket(getDatabaseTicket.Result.DeskproId, cancellationToken);
        if (deskproTicket.IsError) throw new BusinessException($"Error getting Deskpro ticket {getDatabaseTicket.Result.DeskproId}.");

        var agent = deskproTicket.Value?.Agent?.Id != null
            ? await deskpro.GetPersonById(deskproTicket.Value.Agent.Id, cancellationToken) 
            : Error.NotFound().ToErrorOr<PersonDto>();

        var person = deskproTicket.Value?.Person?.Id != null
            ? await deskpro.GetPersonById(deskproTicket.Value.Person.Id, cancellationToken)
            : Error.NotFound().ToErrorOr<PersonDto>();

        var caseNumber = getPodioItem.Result.Value.Fields.GetValue<string>(podioCaseNumberFieldId);
        
        var payload = new
        {
            SagsNummer = caseNumber,
            agent.Value.Email,
            Navn = agent.Value.FullName,
            PodioID = job.PodioItemId,
            DeskproID = deskproTicket.Value?.Id,
            Titel = deskproTicket.Value?.Subject,
            IndsenderNavn = person.Value.FullName,
            IndsenderMail = person.Value.Email,
            AktindsigtsDato = deskproTicket.Value?.CreatedAt,
            AktSagsURL = getDatabaseTicket.Result.CaseUrl
        };
        
        await openOrchestrator.AddQueueItem(openOrchestratorQueueName, $"Podio {job.PodioItemId}", payload, cancellationToken);
    }
}