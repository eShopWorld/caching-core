using System;
using System.Threading.Tasks;

namespace Eshopworld.Caching.Core
{
    public struct CacheResult<T>
    {
        public readonly bool HasValue;
        public readonly T Value;

        public CacheResult(T value) : this(true,value) {}

        public CacheResult(bool hasValue, T value)
        {
            this.HasValue = hasValue;
            this.Value = value;
        }

        public T Or(Func<T> alternative)
        {
            return this.HasValue ? Value : alternative();
        }

        public async Task<T> Or(Func<Task<T>> alternative)
        {
            return this.HasValue ? Value : await alternative();
        }

        private static readonly CacheResult<T> miss = new CacheResult<T>(false, default(T));
        public static CacheResult<T> Miss() => miss;

    }
}