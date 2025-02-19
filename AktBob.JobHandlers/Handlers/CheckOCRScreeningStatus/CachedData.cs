using System.Collections.Concurrent;

namespace AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;

internal class CachedData
{
    public ConcurrentDictionary<Guid, Case> Cases { get; set; } = new();
}
