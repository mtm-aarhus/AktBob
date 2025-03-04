namespace AktBob.Deskpro.Contracts;
internal interface IGetMessageHandler
{
    Task<Result<MessageDto>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}