using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;

namespace AktBob.Deskpro.Contracts;
public interface IDeskproModule
{
    void InvokeWebhook(string WebhookId, string Payload);
    Task<Result<IReadOnlyCollection<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken);
    Task<Result<Stream>> DownloadMessageAttachment(string downloadUrl, CancellationToken cancellationToken);
    Task<Result<IReadOnlyCollection<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken);
    Task<Result<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyCollection<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken);
    Task<Result<PersonDto>> GetPerson(int personId, CancellationToken cancellationToken);
    Task<Result<PersonDto>> GetPerson(string email, CancellationToken cancellationToken);
    Task<Result<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyCollection<TicketDto>>> GetTicketsByFieldSearch(int[] fields, string searchValue, CancellationToken cancellationToken);
}