using System.Text.Json;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using ErrorOr;

namespace AktBob.Shared.ModuleClients.DeskproModule;

public interface IDeskproModuleClient
{
    Task<ErrorOr<Stream>> DownloadMessageAttachment(string downloadUrl, CancellationToken cancellationToken = default);
    Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken = default);
    Task<ErrorOr<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken = default);
    Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken = default);
    Task<ErrorOr<IReadOnlyCollection<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken = default);
    Task<ErrorOr<PersonDto>> GetPersonByEmail(string email, CancellationToken cancellationToken = default);
    Task<ErrorOr<PersonDto>> GetPersonById(int personId, CancellationToken cancellationToken = default);
    Task<ErrorOr<TeamDto>> GetTeam(int teamId, CancellationToken cancellationToken = default);
    Task<ErrorOr<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken = default);
    Task<ErrorOr<TicketDto>> SearchTicketsByFields(int[] fields, string searchValue, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> InvokeWebhook(string webhookId, JsonDocument? payload, CancellationToken cancellationToken = default);
}