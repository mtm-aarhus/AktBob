using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public interface IDeskproModule
{
    Task<Result<IEnumerable<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken);
    Task<Result<Stream>> GetMessageAttachment(string downloadUrl, CancellationToken cancellationToken);
    Task<Result<IEnumerable<AttachmentDto>>> GetDeskproMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken);
    void InvokeWebhook(string WebhookId, object Payload);
}
