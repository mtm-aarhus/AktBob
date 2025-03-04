using AktBob.Deskpro.JobHandlers;
using AktBob.Shared;

namespace AktBob.Deskpro;
internal class Module(
    IJobDispatcher jobDispatcher,
    IGetCustomFieldSpecificationsHandler getCustomFieldSpecificationsHandler,
    IGetMessageAttachmentHandler getMessageAttachmentHandler,
    IGetMessageAttachmentsHandler getMessageAttachmentsHandler,
    IGetMessageHandler getMessageHandler,
    IGetMessagesHandler getMessagesHandler,
    IGetPersonHandler getPersonHandler,
    IGetTicketHandler getTicketHandler,
    IGetTicketsByFieldSearchHandler getTicketsByFieldSearchHandler) : IDeskproModule
{
    public void InvokeWebhook(string WebhookId, object Payload) => jobDispatcher.Dispatch(new InvokeWebhookJob(WebhookId, Payload));
    
    public async Task<Result<IEnumerable<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken) => await getCustomFieldSpecificationsHandler.Handle(cancellationToken);

    public async Task<Result<IEnumerable<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken) => await getMessageAttachmentsHandler.Handle(ticketId, messageId, cancellationToken);

    public async Task<Result<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken) => await getMessageHandler.Handle(ticketId, messageId, cancellationToken);

    public async Task<Result<IEnumerable<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken) => await getMessagesHandler.Handle(ticketId, cancellationToken);

    public async Task<Result<Stream>> GetMessageAttachment(string downloadUrl, CancellationToken cancellationToken) => await getMessageAttachmentHandler.Handle(downloadUrl, cancellationToken);

    public async Task<Result<PersonDto>> GetPerson(int personId, CancellationToken cancellationToken) => await getPersonHandler.Handle(personId, cancellationToken);

    public async Task<Result<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken) => await getTicketHandler.Handle(ticketId, cancellationToken);

    public async Task<Result<IEnumerable<TicketDto>>> GetTicketsByFieldSearch(int[] fields, string searchValue, CancellationToken cancellationToken) => await getTicketsByFieldSearchHandler.Handle(fields, searchValue, cancellationToken);
}
