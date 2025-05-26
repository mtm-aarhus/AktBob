namespace AktBob.Deskpro.Handlers.GetTicket;
internal interface IGetTicketHandler
{
    Task<ErrorOr<TicketDto>> Handle(int ticketId, CancellationToken cancellationToken);
}