namespace Aktbob.Modules.Deskpro.Features.GetMessages;
internal interface IGetMessagesHandler
{
    Task<ErrorOr<IReadOnlyCollection<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken);
}