using AktBob.Deskpro.Handlers;
using AktBob.Deskpro.JobHandlers;
using AktBob.Shared;

namespace AktBob.Deskpro;
internal class DeskproModule(
    IJobDispatcher jobDispatcher,
    IGetDeskproCustomFieldSpecificationsHandler getDeskproCustomFieldSpecificationsHandler,
    IGetDeskproMessageAttachmentHandler getDeskproMessageAttachmentHandler,
    IGetDeskproMessageAttachmentsHandler getDeskproMessageAttachmentsHandler) : IDeskproModule
{
    public async Task<Result<IEnumerable<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken) => await getDeskproCustomFieldSpecificationsHandler.Handle(cancellationToken);

    public Task<Result<IEnumerable<AttachmentDto>>> GetDeskproMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken) => getDeskproMessageAttachmentsHandler.Handle(ticketId, messageId, cancellationToken);

    public async Task<Result<Stream>> GetMessageAttachment(string downloadUrl, CancellationToken cancellationToken) => await getDeskproMessageAttachmentHandler.Handle(downloadUrl, cancellationToken);

    public void InvokeWebhook(string WebhookId, object Payload) => jobDispatcher.Dispatch(new InvokeWebhookJob(WebhookId, Payload));
}
