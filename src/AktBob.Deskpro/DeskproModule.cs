﻿using AktBob.Deskpro.Jobs;
using AktBob.Shared;
using System.Text;

namespace AktBob.Deskpro;
internal class DeskproModule(
    IJobDispatcher jobDispatcher,
    IGetCustomFieldSpecificationsHandler getCustomFieldSpecificationsHandler,
    IDownloadMessageAttachmentHandler getMessageAttachmentHandler,
    IGetMessageAttachmentsHandler getMessageAttachmentsHandler,
    IGetMessageHandler getMessageHandler,
    IGetMessagesHandler getMessagesHandler,
    IGetPersonHandler getPersonHandler,
    IGetTicketHandler getTicketHandler,
    IGetTicketsByFieldSearchHandler getTicketsByFieldSearchHandler) : IDeskproModule
{
    public void InvokeWebhook(string webhookId, string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        var base64Payload = Convert.ToBase64String(bytes);
        jobDispatcher.Dispatch(new InvokeWebhookJob(webhookId, base64Payload));
    }
    
    public async Task<Result<IReadOnlyCollection<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken) => await getCustomFieldSpecificationsHandler.Handle(cancellationToken);

    public async Task<Result<IReadOnlyCollection<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken) => await getMessageAttachmentsHandler.Handle(ticketId, messageId, cancellationToken);

    public async Task<Result<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken) => await getMessageHandler.Handle(ticketId, messageId, cancellationToken);

    public async Task<Result<IReadOnlyCollection<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken) => await getMessagesHandler.Handle(ticketId, cancellationToken);

    public async Task<Result<Stream>> DownloadMessageAttachment(string downloadUrl, CancellationToken cancellationToken) => await getMessageAttachmentHandler.Handle(downloadUrl, cancellationToken);

    public async Task<Result<PersonDto>> GetPerson(int personId, CancellationToken cancellationToken) => await getPersonHandler.GetById(personId, cancellationToken);
    public async Task<Result<PersonDto>> GetPerson(string email, CancellationToken cancellationToken) => await getPersonHandler.GetByEmail(email, cancellationToken);

    public async Task<Result<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken) => await getTicketHandler.Handle(ticketId, cancellationToken);

    public async Task<Result<IReadOnlyCollection<TicketDto>>> GetTicketsByFieldSearch(int[] fields, string searchValue, CancellationToken cancellationToken) => await getTicketsByFieldSearchHandler.Handle(fields, searchValue, cancellationToken);
}
