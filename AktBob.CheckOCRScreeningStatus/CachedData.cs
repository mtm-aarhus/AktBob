using System.Collections.Concurrent;

namespace AktBob.CheckOCRScreeningStatus;

internal class CachedData
{
    public ConcurrentDictionary<Guid, Case> Cases { get; set; } = new();
}
