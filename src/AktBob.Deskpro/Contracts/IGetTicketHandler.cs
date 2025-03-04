namespace AktBob.Deskpro.Contracts;
internal interface IGetTicketHandler
{
    Task<Result<TicketDto>> Handle(int ticketId, CancellationToken cancellationToken);
}