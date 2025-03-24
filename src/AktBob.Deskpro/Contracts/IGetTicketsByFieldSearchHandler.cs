namespace AktBob.Deskpro.Contracts;
internal interface IGetTicketsByFieldSearchHandler
{
    Task<Result<IReadOnlyCollection<TicketDto>>> Handle(int[] fields, string searchValue, CancellationToken cancellationToken);
}