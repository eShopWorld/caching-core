using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eshopworld.Caching.Core.Caches
{
    /// <summary>
    /// This cache provides an implementation, where it will attempt to obtain an item from a l1 cache, and if not found, obtain from the l2.
    /// </summary>
    public class HybridCache<T> : ICache<T>
    {
        private readonly ICache<T> l1;
        private readonly ICache<T> l2;
        private readonly TimeSpan firstLevelCacheDuration;

        public HybridCache(ICache<T> l1, ICache<T> l2) : this(l1,l2,TimeSpan.FromHours(1)) {}
        public HybridCache(ICache<T> l1, ICache<T> l2,TimeSpan firstLevelCacheDuration)
        {
            #region Sanitation
            if (l1 == null) throw new ArgumentNullException("l1");
            if (l2 == null) throw new ArgumentNullException("l2");
            #endregion

            this.l1 = l1;
            this.l2 = l2;
            this.firstLevelCacheDuration = firstLevelCacheDuration;
        }

        public T Add(CacheItem<T> item)
        {
            #region Sanitation
            if (item == null) throw new ArgumentNullException(nameof(item));
            #endregion

            l2.Add(item);
            l1.Add(item);

            return item.Value;
        }

        public async Task<T> AddAsync(CacheItem<T> item)
        {
            #region Sanitation
            if (item == null) throw new ArgumentNullException(nameof(item));
            #endregion

            await l2.AddAsync(item);
            await l1.AddAsync(item);

            return item.Value;
        }

        public void Set(CacheItem<T> item)
        {
            #region Sanitation
            if (item == null) throw new ArgumentNullException(nameof(item));
            #endregion

            l2.Set(item);
            l1.Set(item);
        }

        public async Task SetAsync(CacheItem<T> item)
        {
            #region Sanitation
            if (item == null) throw new ArgumentNullException(nameof(item));
            #endregion

            await l2.SetAsync(item);
            await l1.SetAsync(item);
        }

        public void Remove(string key)
        {
            #region Sanitation
            if (key == null) throw new ArgumentNullException(nameof(key));
            #endregion

            try
            {
                l1.Remove(key);
            }
            finally
            {
                // regardless of success / fail, still remove entry from the remote cache
                l2.Remove(key);    
            }
        }

        public async Task RemoveAsync(string key)
        {
            #region Sanitation
            if (key == null) throw new ArgumentNullException(nameof(key));
            #endregion

            try
            {
                await l1.RemoveAsync(key);
            }
            finally
            {
                // regardless of success / fail, still remove entry from the remote cache
                await l2.RemoveAsync(key);
            }
        }

        /// <summary> Only checks the l1 cache if the key exists</summary>
        public bool Exists(string key) => l1.Exists(key);
        public Task<bool> ExistsAsync(string key) => l1.ExistsAsync(key);
        

        public T Get(string key)
        {
            #region Sanitation
            if (key == null) throw new ArgumentNullException(nameof(key));
            #endregion

            return GetResult(key).Value;
        }

        public async Task<T> GetAsync(string key)
        {
            #region Sanitation
            if (key == null) throw new ArgumentNullException(nameof(key));
            #endregion

            return (await GetResultAsync(key)).Value;
        }

        public CacheResult<T> GetResult(string key)
        {
            #region Sanitation
            if (key == null) throw new ArgumentNullException(nameof(key));
            #endregion

            var l1Result = l1.GetResult(key);
            if (l1Result.HasValue) return l1Result;

            var l2Result = l2.GetResult(key);
            if (l2Result.HasValue) l1.Add(new CacheItem<T>(key, l2Result.Value, firstLevelCacheDuration));

            return l2Result;
        }

        public async Task<CacheResult<T>> GetResultAsync(string key)
        {
            #region Sanitation
            if (key == null) throw new ArgumentNullException(nameof(key));
            #endregion

            var l1Result = await l1.GetResultAsync(key);
            if (l1Result.HasValue) return l1Result;

            var l2Result = await l2.GetResultAsync(key);
            if (l2Result.HasValue) await l1.AddAsync(new CacheItem<T>(key, l2Result.Value, firstLevelCacheDuration));

            return l2Result;
        }

        public IEnumerable<KeyValuePair<string, T>> Get(IEnumerable<string> keys)
        {
            #region Sanitation
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            #endregion

            var keysArray = keys.ToArray();
            var l1Results = l1.Get(keysArray).ToArray();
            if (l1Results.Length == keysArray.Length) return l1Results;

            var l1ItemKeys = l1Results.Select(r => r.Key).ToArray();
            var l2Keys = keysArray.Where(k => !l1ItemKeys.Contains(k)).ToArray();
            var l2Results = l2.Get(l2Keys).ToArray();

            foreach (var r in l2Results)
            {
                l1.Add(new CacheItem<T>(r.Key, r.Value, firstLevelCacheDuration));
            }

            return l1Results.Concat(l2Results);
        }

        public async Task<IEnumerable<KeyValuePair<string, T>>> GetAsync(IEnumerable<string> keys)
        {
            #region Sanitation
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            #endregion

            var keysArray = keys.ToArray();
            var l1Results = (await l1.GetAsync(keysArray)).ToArray();
            if (l1Results.Length == keysArray.Length) return l1Results;

            var l1ItemKeys = l1Results.Select(r => r.Key).ToArray();
            var l2Keys = keysArray.Where(k => !l1ItemKeys.Contains(k)).ToArray();
            var l2Results = (await l2.GetAsync(l2Keys)).ToArray();

            foreach (var r in l2Results)
            {
                await l1.AddAsync(new CacheItem<T>(r.Key, r.Value, firstLevelCacheDuration));
            }

            return l1Results.Concat(l2Results);
        }
    }

    public class HybridCacheFactory : ICacheFactory
    {
        private readonly ICacheFactory _l2Factory;
        private readonly TimeSpan _defaultCacheTimeout;
        private readonly ICacheFactory _l1Factory;

        public HybridCacheFactory(ICacheFactory l1Factory,ICacheFactory l2Factory,TimeSpan defaultCacheTimeout)
        {
            _l1Factory = l1Factory ?? throw new ArgumentNullException(nameof(l1Factory));
            _l2Factory = l2Factory ?? throw new ArgumentNullException(nameof(l2Factory));
            _defaultCacheTimeout = defaultCacheTimeout;
        }

        public ICache<T> Create<T>(string name) => new HybridCache<T>(_l1Factory.Create<T>(name), _l2Factory.Create<T>(name), _defaultCacheTimeout);
        public ICache<T> CreateDefault<T>() => new HybridCache<T>(_l1Factory.CreateDefault<T>(), _l2Factory.CreateDefault<T>(), _defaultCacheTimeout);
    }
}