using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
internal interface IGetMessageAttachmentsHandler
{
    Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(MessageId messageId, CancellationToken cancellationToken);
}