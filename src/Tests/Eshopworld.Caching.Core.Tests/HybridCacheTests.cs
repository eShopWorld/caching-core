using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eshopworld.Caching.Core.Caches;
using Moq;
using Xunit;

namespace Eshopworld.Caching.Core.Tests
{
    public class HybridCacheTests
    {
        private const string CacheKey = "item";
        private HybridCacheFactory cacheFactory;
        private HybridCache<string> stringCache;
        private Mock<ICacheFactory> l1CacheFactoryMock;
        private Mock<ICacheFactory> l2CacheFactoryMock;
        private Mock<ICache<string>> l1CacheMock;
        private Mock<ICache<string>> l2CacheMock;
        private TimeSpan L1CacheDefaultDuration;

        public HybridCacheTests()
        {
            l1CacheMock = new Mock<ICache<string>>();
            l2CacheMock = new Mock<ICache<string>>();

            l1CacheFactoryMock = new Mock<ICacheFactory>();
            l1CacheFactoryMock.Setup(call => call.CreateDefault<string>()).Returns(l1CacheMock.Object);

            l2CacheFactoryMock = new Mock<ICacheFactory>();
            l2CacheFactoryMock.Setup(call => call.CreateDefault<string>()).Returns(l2CacheMock.Object);

            L1CacheDefaultDuration = TimeSpan.FromMinutes(1);
            cacheFactory = new HybridCacheFactory(l1CacheFactoryMock.Object, l2CacheFactoryMock.Object, L1CacheDefaultDuration);

            stringCache = (HybridCache<string>)cacheFactory.CreateDefault<string>();
            stringCache.Remove(CacheKey);
        }


        [Fact]
        public void Set_CallsSetForL1AndL2()
        {
            // Arrange
            var cacheItem = new CacheItem<string>(CacheKey, "Test", TimeSpan.FromSeconds(5));
            // Act

            stringCache.Set(cacheItem);
            // Assert
            l1CacheMock.Verify(call => call.Set(cacheItem));
            l2CacheMock.Verify(call => call.Set(cacheItem));
        }

        [Fact]
        public async Task SetAsync_CallsSetAsyncForL1AndL2()
        {
            // Arrange
            var cacheItem = new CacheItem<string>(CacheKey, "Test", TimeSpan.FromSeconds(5));

            // Act
            await stringCache.SetAsync(cacheItem);
            // Assert

            l1CacheMock.Verify(call => call.SetAsync(cacheItem));
            l2CacheMock.Verify(call => call.SetAsync(cacheItem));
        }

        [Fact]
        public void Add_CallsAddForL1AndL2()
        {
            // Arrange
            var cacheItem = new CacheItem<string>(CacheKey, "Test", TimeSpan.FromSeconds(5));
            // Act

            stringCache.Add(cacheItem);
            // Assert
            l1CacheMock.Verify(call => call.Add(cacheItem));
            l2CacheMock.Verify(call => call.Add(cacheItem));
        }

        [Fact]
        public async Task AddAsync_CallsAddAsyncForL1AndL2()
        {
            // Arrange
            var cacheItem = new CacheItem<string>(CacheKey, "Test", TimeSpan.FromSeconds(5));

            // Act
            await stringCache.AddAsync(cacheItem);
            // Assert

            l1CacheMock.Verify(call => call.AddAsync(cacheItem));
            l2CacheMock.Verify(call => call.AddAsync(cacheItem));
        }

        [Fact]
        public void Get_WithItemInL1Cache_ReturnsItemAndDoesntCallL2()
        {
            // Arrange
            var cacheValue = "Test";
            l1CacheMock.Setup(call => call.GetResult(CacheKey)).Returns(new CacheResult<string>(cacheValue));

            // Act
            var result = stringCache.Get(CacheKey);

            // Assert
            Assert.Equal(cacheValue,result);
            l2CacheMock.Verify(call => call.Get(CacheKey), Times.Never());
        }

        [Fact]
        public void Get_WithItemNotL1Cache_CallsL2CacheAndAddsToL1Cache()
        {
            // Arrange
            var cacheValue = "Test";
            l1CacheMock.Setup(call => call.GetResult(CacheKey)).Returns(CacheResult<string>.Miss());
            l2CacheMock.Setup(call => call.GetResult(CacheKey)).Returns(new CacheResult<string>(cacheValue));

            // Act
            var result = stringCache.Get(CacheKey);

            // Assert
            Assert.Equal(cacheValue, result);
            l2CacheMock.Verify(call => call.GetResult(CacheKey));
            l1CacheMock.Verify(call => call.Add(It.IsAny<CacheItem<string>>()));
        }

        [Fact]
        public void GetResult_WithItemNotL1CacheOrL2Cache_ResultDoesNotHaveValue()
        {
            // Arrange
            l1CacheMock.Setup(call => call.GetResult(CacheKey)).Returns(CacheResult<string>.Miss());
            l2CacheMock.Setup(call => call.GetResult(CacheKey)).Returns(CacheResult<string>.Miss());

            // Act
            var result = stringCache.GetResult(CacheKey);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasValue);
        }

        [Fact]
        public void Remove_RemovesFromL1AndL2()
        {
            // Arrange
            // Act
            stringCache.Remove(CacheKey);

            // Assert
            l1CacheMock.Verify(call => call.Remove(CacheKey));
            l2CacheMock.Verify(call => call.Remove(CacheKey));
        }


        [Fact]
        public void GetCollection_AllL1CacheMissesAreFetchedFromL2AndAddedToL1()
        {
            // Arrange
            var keys = new [] {"a", "b", "c","d"};

            l1CacheMock.Setup(call => call.Get(keys)).Returns(new []{new KeyValuePair<string,string>("a","1"), new KeyValuePair<string, string>("b", "2") });
            l2CacheMock.Setup(call => call.Get(It.IsAny<IEnumerable<string>>())).Returns(new[] { new KeyValuePair<string, string>("c", "3"), new KeyValuePair<string, string>("d", "4") });

            // Act
            stringCache.Get(keys);

            // Assert
            l2CacheMock.Verify(call => call.Get(It.Is<IEnumerable<string>>(en => en.ToArray().SequenceEqual(new [] {"c","d"}))));
            l1CacheMock.Verify(call => call.Add(It.Is<CacheItem<string>>(ci => ci.Key == "c" && ci.Value == "3" && ci.Duration == L1CacheDefaultDuration)));
            l1CacheMock.Verify(call => call.Add(It.Is<CacheItem<string>>(ci => ci.Key == "d" && ci.Value == "4" && ci.Duration == L1CacheDefaultDuration)));
        }

        [Fact]
        public void GetCollection_WithItemsFromL1AndL2CacheAggregateResultIsReturned()
        {
            // Arrange
            var keys = new [] { "a", "b", "c", "d" };

            l1CacheMock.Setup(call => call.Get(keys)).Returns(new[] { new KeyValuePair<string, string>("a", "1"), new KeyValuePair<string, string>("b", "2") });
            l2CacheMock.Setup(call => call.Get(It.IsAny<IEnumerable<string>>())).Returns(new[] { new KeyValuePair<string, string>("c", "3"), new KeyValuePair<string, string>("d", "4") });

            // Act
            var result = stringCache.Get(keys);

            // Assert
            Assert.NotNull(result);
            var resultToArray = result.ToArray();
            Assert.True(resultToArray.Select(r => r.Key).SequenceEqual(keys));
        }
    }
}