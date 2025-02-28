using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessageAttachmentsQuery(int TicketId, int MessageId) : IQuery<Result<IEnumerable<AttachmentDto>>>;