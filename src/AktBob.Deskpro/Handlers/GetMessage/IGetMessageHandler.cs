using AktBob.Shared.Types.Deskpro;

namespace AktBob.Deskpro.Handlers.GetMessage;
internal interface IGetMessageHandler
{
    Task<ErrorOr<MessageDto>> Handle(MessageId messageId, CancellationToken cancellationToken);
}