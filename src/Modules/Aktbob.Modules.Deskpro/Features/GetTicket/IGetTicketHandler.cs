using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetTicket;
internal interface IGetTicketHandler
{
    Task<ErrorOr<TicketDto>> Handle(TicketId ticketId, CancellationToken cancellationToken);
}