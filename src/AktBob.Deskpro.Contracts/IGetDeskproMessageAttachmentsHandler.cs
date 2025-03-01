using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public interface IGetDeskproMessageAttachmentsHandler
{
    Task<Result<IEnumerable<AttachmentDto>>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}