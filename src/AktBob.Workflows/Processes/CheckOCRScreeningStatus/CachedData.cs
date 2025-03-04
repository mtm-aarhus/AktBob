using System.Collections.Concurrent;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

internal class CachedData
{
    public ConcurrentDictionary<Guid, Case> Cases { get; set; } = new();
}
