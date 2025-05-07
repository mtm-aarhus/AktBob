namespace AktBob.Database.Contracts;
public interface IUnitOfWork
{
    IMessageRepository Messages { get; }
    ITicketRepository Tickets { get; }
    ICaseRepository Cases { get; }
    IOS2FormsSubmissionRepository OS2FormsSubmissions { get; }
    IFilArkivFilesCleanUpQueueRepository FilArkivFilesCleanUpQueue { get; }
}
