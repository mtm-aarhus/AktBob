using System.Collections.Concurrent;

namespace AktBob.JobHandlers.Processes.CheckOCRScreeningStatus;

internal class CachedData
{
    public ConcurrentDictionary<Guid, Case> Cases { get; set; } = new();
}
