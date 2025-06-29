using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetTicket;
public interface IGetTicketHandler
{
    Task<ErrorOr<TicketDto>> Handle(int ticketId, CancellationToken cancellationToken);
}