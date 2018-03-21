namespace Eshopworld.Caching.Core
{
    /// <summary>
    /// identifies a cache which maintains a shared state between multiple client nodes
    /// </summary>
    /// <typeparam name="T">type of item to cache</typeparam>
    public interface IDistributedCache<T> : ICache<T>
    {
        
    }
}