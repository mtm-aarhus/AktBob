namespace AktBob.GetOrganized.Contracts;
public interface IFinalizeGetOrganizedDocumentHandler
{
    Task Handle(int documentId, bool shouldCloseOpenTasks = false, CancellationToken cancellationToken = default);
}