namespace AktBob.Database.Contracts;
public interface IUnitOfWork
{
    IMessageRepository Messages { get; }
    ITicketRepository Tickets { get; }
    ICaseRepository Cases { get; }
}
