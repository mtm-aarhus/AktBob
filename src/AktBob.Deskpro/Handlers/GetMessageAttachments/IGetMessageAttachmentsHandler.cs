using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetMessageAttachments;
internal interface IGetMessageAttachmentsHandler
{
    Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}