using Microsoft.Extensions.Caching.Memory;

namespace DataProcessingService.Interfaces;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T GetOrAdd<T>(string key, Func<ICacheEntry, T> factory)
    {
        return _cache.GetOrCreate(key, factory);
    }

    public async Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> factory)
    {
        if (_cache.TryGetValue(key, out T value))
        {
            return value;
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions();
        value = await factory((ICacheEntry)cacheEntryOptions);
        _cache.Set(key, value, cacheEntryOptions);
        return value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
