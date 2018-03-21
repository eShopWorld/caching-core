using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eshopworld.Caching.Core.Caches
{
    /// <summary>
    /// This cache provides an dictionary backing store to cache items, and will randomly evict items once the maxNumberOfCacheItems is exceeded 
    /// Needless to say, this cache should generally not be used for production
    /// </summary>
    public class DictionaryCache<T> : ILocalCache<T>
    {
        public const int DefaultMaxNumberOfItems = 1000;

        private readonly IDictionary<string, T> cache;
        public int MaxNumberOfCacheItems { get; private set; }

        public DictionaryCache() : this(new Dictionary<string, T>(), DefaultMaxNumberOfItems){}

        public DictionaryCache(IDictionary<string, T> cache,int maxNumberOfCacheItems)
        {
            #region Sanitation
            if (cache == null) throw new ArgumentNullException("cache");
            if (maxNumberOfCacheItems < 1) throw new ArgumentOutOfRangeException("maxNumberOfCacheItems", "value must not be less than 1");
            #endregion

            this.cache = cache;
            this.MaxNumberOfCacheItems = maxNumberOfCacheItems;
        }



        #region IObjectCache

        public T Add(CacheItem<T> item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            EnsureThreshold();

            cache.Add(item.Key, item.Value);
            return item.Value;
        }

        public Task<T> AddAsync(CacheItem<T> item) => Task.FromResult(Add(item));

        public void Set(CacheItem<T> item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            cache[item.Key] = item.Value;
        }

        public Task SetAsync(CacheItem<T> item)
        {
            Set(item);
            return Task.FromResult(0);
        }

        public void Remove(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            cache.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.FromResult(0);
        }

        public bool Exists(string key) => cache.ContainsKey(key);
        public Task<bool> ExistsAsync(string key) => Task.FromResult(Exists(key));

        

        public T Get(string key)
        {
            #region Sanitation

            if (key == null) throw new ArgumentNullException("key");

            #endregion

            T result;

            cache.TryGetValue(key, out result);

            return result;
        }

        public Task<T> GetAsync(string key) => Task.FromResult(Get(key));

        public CacheResult<T> GetResult(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return cache.ContainsKey(key) ? new CacheResult<T>(true, cache[key]) : CacheResult<T>.Miss();
        }

        public Task<CacheResult<T>> GetResultAsync(string key) => Task.FromResult(GetResult(key));


        public IEnumerable<KeyValuePair<string, T>> Get(IEnumerable<string> keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            return cache.Where(kvp => keys.Contains(kvp.Key));
        }

        public Task<IEnumerable<KeyValuePair<string, T>>> GetAsync(IEnumerable<string> keys) => Task.FromResult(Get(keys));

        #endregion



        public void Clear()
        {
            cache.Clear();
        }

        public static DictionaryCache<T> CreateDefault()
        {
            return new DictionaryCache<T>(new Dictionary<string, T>(), DefaultMaxNumberOfItems);
        }


        #region Private Methods

        private void EnsureThreshold()
        {
            while (cache.Count > MaxNumberOfCacheItems)
            {
                cache.Remove(cache.First().Key);
            }
        }

        #endregion


    }
}