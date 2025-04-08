using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AktBob.Shared;

public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan duration);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        _logger.LogDebug("Getting cache value of type {type} for key {key}", typeof(T), key);
        _memoryCache.TryGetValue(key, out T? value);

        if (value == null)
        {
            _logger.LogDebug("Cached value of type {type} for key {key} not found", typeof(T), key);
        }

        return value;
    }

    public void Set<T>(string key, T value, TimeSpan duration)
    {
        _logger.LogDebug("Setting cached value with key {key} of type {type}: {value}. Expires in {duration}", key, typeof(T), value, duration);
        _memoryCache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duration
        });
    }
}
