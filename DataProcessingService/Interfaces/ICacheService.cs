using Microsoft.Extensions.Caching.Memory;

namespace DataProcessingService.Interfaces;

public interface ICacheService
{
    T GetOrAdd<T>(string key, Func<ICacheEntry, T> factory);
    Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> factory);
    void Remove(string key);
}
