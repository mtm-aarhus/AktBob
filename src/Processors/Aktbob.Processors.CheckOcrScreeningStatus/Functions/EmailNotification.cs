using Aktbob.Processors.CheckOcrScreeningStatus.Jobs;
using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using AktBob.Shared.ModuleClients.DeskproModule;
using AktBob.Shared.ModuleClients.PodioModule;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Functions;

public class EmailNotification(
    ILogger<EmailNotification> logger,
    IConfiguration configuration,
    IPodioModuleClient podio,
    IDeskproModuleClient deskpro,
    IMessageBus messageBus)
{
    [Function("email-notification")]
    public async Task Run(
        [ServiceBusTrigger("%QueueNames:EmailNotification%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        var job = MessageDeserializer.Deserialize<EmailNotificationJob>(message);

        var (success, deadLetterReason, deadLetterDescription, recipient, caseNumber, ticketId) = await GetPodioFieldValues(job.PodioItemId, cancellationToken);
        if (!success)
        {
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: deadLetterReason, deadLetterErrorDescription: deadLetterDescription, cancellationToken: cancellationToken);
            return;
        }
        
        // Get Deskpro ticket
        var ticketSubject = await GetDeskproSubject(ticketId, cancellationToken);

        // Notify
        await EnqueueNotification(caseNumber, ticketSubject, ticketId, job.FilArkivCaseId, recipient!, cancellationToken);
    }

    private async Task<(bool success, string deadLetterReason, string deadLetterDescription, string? recipient, string caseNumber, string ticketId)> GetPodioFieldValues(long podioItemId, CancellationToken cancellationToken)
    {
        var podioAppId = Guard.Against.Null(configuration.GetValue<int?>("Podio:AppId"));
        var sagsansvarligEmailFieldId = Guard.Against.Null(configuration.GetValue<int?>("Podio:Fields:Sagsansvarlig"));
        var caseNumberFieldId = Guard.Against.Null(configuration.GetValue<int?>("Podio:Fields:CaseNumber"));
        var deskproIdFieldId = Guard.Against.Null(configuration.GetValue<int?>("Podio:Fields:DeskproId"));
        
        var podioItemResult = await podio.GetItem(podioAppId, podioItemId, cancellationToken);
        if (podioItemResult.IsError)
        {
            logger.LogError("Error getting Podio item {podioItemId}: {errors}. Moving message to DLQ.", podioItemId, podioItemResult.Errors.ToCommaDelimitedString());
            return (success: false, deadLetterReason: $"Error getting Podio item {podioItemId}", deadLetterDescription: podioItemResult.Errors.ToCommaDelimitedString(), recipient: string.Empty, caseNumber: string.Empty, ticketId: string.Empty);
        }

        var recipient = podioItemResult.Value.Fields.GetValue<string>(sagsansvarligEmailFieldId);
        var caseNumber = podioItemResult.Value.Fields.GetValue<string>(caseNumberFieldId) ?? "IKKE ANGIVET";
        var ticketId = podioItemResult.Value.Fields.GetValue<string>(deskproIdFieldId) ?? "IKKE ANGIVET";
        
        if (string.IsNullOrEmpty(recipient))
        {
            logger.LogError("Recipient could not be found from Podio item {podioItemId}. Moving message to DLQ.", podioItemId);
            return (success: false, deadLetterReason: $"Error getting Podio item {podioItemId}", deadLetterDescription: string.Empty, recipient: string.Empty, caseNumber: string.Empty, ticketId: string.Empty);

        }

        return (success: true, deadLetterReason: string.Empty, deadLetterDescription: string.Empty, recipient, caseNumber, ticketId);
    }

    private async Task<string> GetDeskproSubject(string ticketId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(ticketId, default, out var deskproTicketId)) return string.Empty;
        var getDeskproTicket = await deskpro.GetTicket(deskproTicketId, cancellationToken);
        return !getDeskproTicket.IsError ? getDeskproTicket.Value.Subject : string.Empty;
    }

    private async Task EnqueueNotification(string caseNumber, string ticketSubject, string ticketId, Guid filArkivCaseId, string recipient, CancellationToken cancellationToken)
    {
        var notificationQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("QueueNames:Notification"));
        
        var subject = $"{caseNumber}: Screening færdig";
        var fields = new Dictionary<string, string>
        {
            { "caseNumber", caseNumber },
            { "ticketSubject", ticketSubject },
            { "ticketId", ticketId },
            { "linkFilArkivCase", $"https://aarhus.filarkiv.dk/archives/case/{filArkivCaseId}" }
        };

        const string template = "ocr-screening-finished.html";
        var notificationJob = new NotificationJob(recipient, template, subject, fields);
        await messageBus.SendMessage(notificationQueueName, notificationJob, cancellationToken);
    }
}