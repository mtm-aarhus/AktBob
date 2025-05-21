namespace AktBob.Deskpro.Handlers.GetMessage;
internal interface IGetMessageHandler
{
    Task<ErrorOr<MessageDto>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}