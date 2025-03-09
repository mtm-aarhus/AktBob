using System.Collections.Concurrent;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

internal sealed class CachedData
{
    private static readonly Lazy<CachedData> _instance = new(() => new());
    public static CachedData Instance => _instance.Value;
    public ConcurrentDictionary<Guid, Case> Cases { get; set; } = new();
}
