using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetMessage;
internal interface IGetMessageHandler
{
    Task<ErrorOr<MessageDto>> Handle(MessageId messageId, CancellationToken cancellationToken);
}