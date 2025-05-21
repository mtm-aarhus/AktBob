using AktBob.Shared.Types.Deskpro;

namespace AktBob.Deskpro.Handlers.GetMessageAttachments;
internal interface IGetMessageAttachmentsHandler
{
    Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(MessageId messageId, CancellationToken cancellationToken);
}