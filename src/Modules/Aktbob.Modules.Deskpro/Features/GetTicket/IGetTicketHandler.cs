namespace Aktbob.Modules.Deskpro.Features.GetTicket;
internal interface IGetTicketHandler
{
    Task<ErrorOr<TicketDto>> Handle(int ticketId, CancellationToken cancellationToken);
}