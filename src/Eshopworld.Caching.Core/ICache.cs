using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eshopworld.Caching.Core
{
    public interface ICache<T>
    {
        T Add(CacheItem<T> item);
        Task<T> AddAsync(CacheItem<T> item);

        void Set(CacheItem<T> item);
        Task SetAsync(CacheItem<T> item);

        void Remove(string key);
        Task RemoveAsync(string key);

        bool Exists(string key);
        Task<bool> ExistsAsync(string key);

        T Get(string key);
        Task<T> GetAsync(string key);

        CacheResult<T> GetResult(string key);
        Task<CacheResult<T>> GetResultAsync(string key);

        IEnumerable<KeyValuePair<string, T>> Get(IEnumerable<string> keys);
        Task<IEnumerable<KeyValuePair<string, T>>> GetAsync(IEnumerable<string> keys);
    }
}