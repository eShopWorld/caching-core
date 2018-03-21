using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eshopworld.Caching.Core.Caches
{
    /// <summary>
    /// This cache provides an implementation, where it will never cache an item, effectivally disabling the cache 
    /// </summary>
    public class NullCache<T> : ICache<T>
    {
        public T Add(CacheItem<T> item) => item.Value;
        public Task<T> AddAsync(CacheItem<T> item) => Task.FromResult(Add(item));


        public void Set(CacheItem<T> item)
        {
            // dear sonar - this is intentionally empty, as its a class that does nothing
        }
        public Task SetAsync(CacheItem<T> item) => Task.FromResult(0);


        public void Remove(string key)
        {
            // dear sonar - this is intentionally empty, as its a class that does nothing
        }
        public Task RemoveAsync(string key) => Task.FromResult(0);

        public bool Exists(string key) => false;
        public Task<bool> ExistsAsync(string key) => Task.FromResult(Exists(key));


        public T Get(string key) => default(T);
        public Task<T> GetAsync(string key) => Task.FromResult(Get(key));
        

        public CacheResult<T> GetResult(string key) => CacheResult<T>.Miss();
        public Task<CacheResult<T>> GetResultAsync(string key) => Task.FromResult(GetResult(key));

        public IEnumerable<KeyValuePair<string, T>> Get(IEnumerable<string> keys) => Enumerable.Empty<KeyValuePair<string, T>>();
        public Task<IEnumerable<KeyValuePair<string, T>>> GetAsync(IEnumerable<string> keys) => Task.FromResult(Get(keys));
    }
}