using AktBob.Database.Contracts;

namespace AktBob.Database;
internal class UnitOfWork : IUnitOfWork
{
    private readonly IFilArkivFilesCleanUpQueueRepository _filArkivFilesCleanUpQueueRepository;

    public UnitOfWork(
        IMessageRepository messageRepository,
        ITicketRepository ticketRepository,
        ICaseRepository caseRepository,
        IOS2FormsSubmissionRepository os2FormsSubmissionRepository,
        IFilArkivFilesCleanUpQueueRepository filArkivFilesCleanUpQueueRepository)
    {
        Messages = messageRepository;
        Tickets = ticketRepository;
        Cases = caseRepository;
        OS2FormsSubmissions = os2FormsSubmissionRepository;
        FilArkivFilesCleanUpQueue = filArkivFilesCleanUpQueueRepository;
    }

    public IMessageRepository Messages { get; }
    public ITicketRepository Tickets { get; }
    public ICaseRepository Cases { get; }
    public IOS2FormsSubmissionRepository OS2FormsSubmissions { get; }
    public IFilArkivFilesCleanUpQueueRepository FilArkivFilesCleanUpQueue { get; }
}
