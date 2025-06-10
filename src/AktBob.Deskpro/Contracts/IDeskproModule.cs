using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public interface IDeskproModule
{
    void InvokeWebhook(string WebhookId, string Payload);
    Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken);
    Task<ErrorOr<Stream>> DownloadMessageAttachment(string downloadUrl, CancellationToken cancellationToken);
    Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken);
    Task<ErrorOr<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken);
    Task<ErrorOr<IReadOnlyCollection<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken);
    Task<ErrorOr<PersonDto>> GetPersonById(int personId, CancellationToken cancellationToken);
    Task<ErrorOr<PersonDto>> GetPersonByEmail(string email, CancellationToken cancellationToken);
    Task<ErrorOr<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken);
    Task<ErrorOr<IReadOnlyCollection<TicketDto>>> GetTicketsByFieldSearch(int[] fields, string searchValue, CancellationToken cancellationToken);
    Task<ErrorOr<TeamDto>> GetTeam(int teamId, CancellationToken cancellationToken);
}