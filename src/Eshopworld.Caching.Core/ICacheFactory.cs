namespace Eshopworld.Caching.Core
{
    public interface ICacheFactory
    {
        ICache<T> Create<T>(string name);
        ICache<T> CreateDefault<T>();
    }
}