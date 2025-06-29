using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
public interface IGetMessageAttachmentsHandler
{
    Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}