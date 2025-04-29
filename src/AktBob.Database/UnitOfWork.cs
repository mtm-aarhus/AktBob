using AktBob.Database.Contracts;

namespace AktBob.Database;
internal class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(
        IMessageRepository messageRepository,
        ITicketRepository ticketRepository,
        ICaseRepository caseRepository,
        IOS2FormsSubmissionRepository os2FormsSubmissionRepository)
    {
        Messages = messageRepository;
        Tickets = ticketRepository;
        Cases = caseRepository;
        OS2FormsSubmissions = os2FormsSubmissionRepository;
    }

    public IMessageRepository Messages { get; }
    public ITicketRepository Tickets { get; }
    public ICaseRepository Cases { get; }
    public IOS2FormsSubmissionRepository OS2FormsSubmissions { get; }
}
