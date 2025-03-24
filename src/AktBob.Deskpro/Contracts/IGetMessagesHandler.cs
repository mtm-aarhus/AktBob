namespace AktBob.Deskpro.Contracts;
internal interface IGetMessagesHandler
{
    Task<Result<IReadOnlyCollection<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken);
}