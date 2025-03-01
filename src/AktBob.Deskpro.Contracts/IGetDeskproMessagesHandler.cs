using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public interface IGetDeskproMessagesHandler
{
    Task<Result<IEnumerable<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken);
}