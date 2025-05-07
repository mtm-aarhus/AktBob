using AktBob.Database.Contracts;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Decorators;
internal class FilArkivFilesCleanUpQueueRepositoryExceptionDecorator : IFilArkivFilesCleanUpQueueRepository
{
    private readonly IFilArkivFilesCleanUpQueueRepository _inner;
    private readonly ILogger<FilArkivFilesCleanUpQueueRepositoryExceptionDecorator> _logger;

    public FilArkivFilesCleanUpQueueRepositoryExceptionDecorator(
        IFilArkivFilesCleanUpQueueRepository inner,
        ILogger<FilArkivFilesCleanUpQueueRepositoryExceptionDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task Add(Guid filArkivFileId)
    {
        try
        {
            await _inner.Add(filArkivFileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Add));
            throw;
        }
    }
}
