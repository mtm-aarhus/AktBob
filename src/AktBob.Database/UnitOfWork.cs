using AktBob.Database.Contracts;

namespace AktBob.Database;
internal class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(IMessageRepository messageRepository, ITicketRepository ticketRepository, ICaseRepository caseRepository)
    {
        Messages = messageRepository;
        Tickets = ticketRepository;
        Cases = caseRepository;
    }

    public IMessageRepository Messages { get; }
    public ITicketRepository Tickets { get; }
    public ICaseRepository Cases { get; }
}
