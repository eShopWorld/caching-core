#if AUTOFAC
using Moq;
using Xunit;
using global::Autofac;

namespace Eshopworld.Caching.Core.Tests.Unit
{
    public class CacheRegistrationSourceTests
    {
        public interface ITestCacheFactory : ICacheFactory { }

        [Fact]
        public void ForOpenGenericType_CallsFactoryMethodWithClosedGenericArgument()
        {
            // Arrange
            var containerBuilder = new ContainerBuilder();
            var cfMock = new Mock<ITestCacheFactory>();
            var cache = new Mock<ICache<string>>().Object;

            cfMock.Setup(call => call.CreateDefault<string>()).Returns(cache);
            containerBuilder.RegisterInstance(cfMock.Object).As<ITestCacheFactory>(); // register our mock cache factory with the container. The CacheRegistrationSource will resove this

            // Act
            containerBuilder.RegisterSource(new Autofac.CacheRegistrationSource<ITestCacheFactory>(typeof(ICache<>))); // now register the resolver. this should resolve all requests for the open generic ICache<> to the factory of the specified type
            var container = containerBuilder.Build();
            var stringCache = container.Resolve<ICache<string>>();

            // Assert
            cfMock.Verify(call => call.CreateDefault<string>());
            Assert.Same(cache, stringCache);
        }
    }
}
#endif