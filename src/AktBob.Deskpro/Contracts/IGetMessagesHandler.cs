using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
internal interface IGetMessagesHandler
{
    Task<Result<IEnumerable<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken);
}