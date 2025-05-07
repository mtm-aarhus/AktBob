namespace AktBob.Database.Contracts;
public interface IFilArkivFilesCleanUpQueueRepository
{
    Task Add(Guid filArkivFileId);
}
