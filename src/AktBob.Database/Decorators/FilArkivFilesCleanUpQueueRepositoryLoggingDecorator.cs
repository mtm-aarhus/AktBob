using AktBob.Database.Contracts;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Decorators;
internal class FilArkivFilesCleanUpQueueRepositoryLoggingDecorator : IFilArkivFilesCleanUpQueueRepository
{
    private readonly ILogger<FilArkivFilesCleanUpQueueRepositoryLoggingDecorator> _logger;
    private readonly IFilArkivFilesCleanUpQueueRepository _inner;

    public FilArkivFilesCleanUpQueueRepositoryLoggingDecorator(
        IFilArkivFilesCleanUpQueueRepository inner,
        ILogger<FilArkivFilesCleanUpQueueRepositoryLoggingDecorator> logger)
    {
        _logger = logger;
        _inner = inner;
    }

    public async Task Add(Guid filArkivFileId)
    {
        _logger.LogInformation("Adding FilArkivFileId {id} to FilArkivFilesCleanUpQueue", filArkivFileId);
        await _inner.Add(filArkivFileId);
    }
}
