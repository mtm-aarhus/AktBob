using Aktbob.Modules.Deskpro.Features.GetTicket;
using Aktbob.Modules.Podio.Features.GetItem;
using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Podio;
using Ardalis.GuardClauses;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Features.EmailNotification;

internal class EmailNotificationHandler(
    ILogger<EmailNotificationHandler> logger,
    IConfiguration configuration,
    IGetItemHandler podioGetItemHandler,
    IGetTicketHandler deskproGetTicketHandler,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<Success>> Run(EmailNotificationJob job, CancellationToken cancellationToken)
    {
        var (success, errorReason, errorDescription, recipient, caseNumber, ticketId) = await GetPodioFieldValues(job.PodioItemId, cancellationToken);
        if (!success)
        {
            return Error.Failure(errorReason, errorDescription);
        }
        
        // Get Deskpro ticket
        var ticketSubject = await GetDeskproSubject(ticketId, cancellationToken);

        // Notify
        await EnqueueNotification(caseNumber, ticketSubject, ticketId, job.FilArkivCaseId, recipient!, cancellationToken);

        return Result.Success;
    }

    private async Task<(bool success, string errorReason, string errorDescription, string? recipient, string caseNumber, string ticketId)> GetPodioFieldValues(long podioItemId, CancellationToken cancellationToken)
    {
        var podioAppId = Guard.Against.Null(configuration.GetValue<int?>("Podio:AppId"));
        var sagsansvarligEmailFieldId = Guard.Against.Null(configuration.GetValue<int?>("Podio:Fields:Sagsansvarlig"));
        var caseNumberFieldId = Guard.Against.Null(configuration.GetValue<int?>("Podio:Fields:CaseNumber"));
        var deskproIdFieldId = Guard.Against.Null(configuration.GetValue<int?>("Podio:Fields:DeskproId"));
        
        var podioItemResult = await podioGetItemHandler.Handle(ItemId.Create(podioAppId, podioItemId), cancellationToken);
        if (podioItemResult.IsError)
        {
            logger.LogError("Error getting Podio item {podioItemId}: {errors}. Moving message to DLQ.", podioItemId, podioItemResult.Errors.ToCommaDelimitedString());
            return (success: false, errorReason: $"Error getting Podio item {podioItemId}", errorDescription: podioItemResult.Errors.ToCommaDelimitedString(), recipient: string.Empty, caseNumber: string.Empty, ticketId: string.Empty);
        }

        var recipient = podioItemResult.Value.Fields.GetValue<string>(sagsansvarligEmailFieldId);
        var caseNumber = podioItemResult.Value.Fields.GetValue<string>(caseNumberFieldId) ?? "IKKE ANGIVET";
        var ticketId = podioItemResult.Value.Fields.GetValue<string>(deskproIdFieldId) ?? "IKKE ANGIVET";
        
        if (string.IsNullOrEmpty(recipient))
        {
            logger.LogError("Recipient could not be found from Podio item {podioItemId}. Moving message to DLQ.", podioItemId);
            return (success: false, errorReason: $"Error getting Podio item {podioItemId}", errorDescription: string.Empty, recipient: string.Empty, caseNumber: string.Empty, ticketId: string.Empty);

        }

        return (success: true, errorReason: string.Empty, errorDescription: string.Empty, recipient, caseNumber, ticketId);
    }

    private async Task<string> GetDeskproSubject(string ticketId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(ticketId, default, out var deskproTicketId)) return string.Empty;
        var getDeskproTicket = await deskproGetTicketHandler.Handle(deskproTicketId, cancellationToken);
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

        const string template = "ocr-screening-finished";
        var notificationJob = new NotificationJob(recipient, template, subject, fields);
        await messageBus.SendMessage(notificationQueueName, notificationJob, cancellationToken);
    }
}