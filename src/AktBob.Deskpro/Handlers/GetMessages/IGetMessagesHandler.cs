using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetMessages;
internal interface IGetMessagesHandler
{
    Task<ErrorOr<IReadOnlyCollection<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken);
}