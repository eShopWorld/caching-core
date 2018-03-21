namespace Eshopworld.Caching.Core
{
    /// <summary>
    /// identifies a cache which caches items locally (i.e. in RAM) on a node. it DOES NOT perform any cache coherence solution
    /// </summary>
    /// <typeparam name="T">type of item to cache</typeparam>
    /// <remarks>This cache should only be used to store infrequently changing data, which is immutable. A good example would be a list of countries / currencies etc</remarks>
    public interface ILocalCache<T> : ICache<T>
    {

    }
}