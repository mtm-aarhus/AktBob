using System.Collections.Concurrent;

namespace AktBob.Workflows.Processes.CreateDocumentListQueueItem;

internal class ReschedulingCounter
{
    private static readonly Lazy<ReschedulingCounter> _instance = new(() => new());
    public static ReschedulingCounter Instance => _instance.Value;
    private readonly ConcurrentDictionary<PodioItemId, int> _counter = new();
    
    public int IncrementAndGet(PodioItemId key) => _counter.AddOrUpdate(key, 1, (_, count) => count + 1);
    
    public void Remove(PodioItemId key)
    {
        _counter.TryRemove(key, out _);
    }
}