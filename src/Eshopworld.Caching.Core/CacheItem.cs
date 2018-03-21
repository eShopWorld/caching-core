using System;

namespace Eshopworld.Caching.Core
{
    public class CacheItem<T>
    {
        public string Key { get; }
        public T Value { get; }
        public TimeSpan Duration { get; }

        [System.Diagnostics.DebuggerStepThrough]
        public CacheItem(string key, T value,TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            this.Key = key;
            this.Value = value;
            this.Duration = duration;
        }
    }
}