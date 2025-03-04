namespace AktBob.Deskpro.Contracts;
internal interface IGetTicketsByFieldSearchHandler
{
    Task<Result<IEnumerable<TicketDto>>> Handle(int[] fields, string searchValue, CancellationToken cancellationToken);
}