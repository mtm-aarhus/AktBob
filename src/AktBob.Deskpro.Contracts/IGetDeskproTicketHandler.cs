using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public interface IGetDeskproTicketHandler
{
    Task<Result<TicketDto>> Handle(int ticketId, CancellationToken cancellationToken);
}