using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Clothy.Shared.Cache
{
    public class EntityCacheService : IEntityCacheService, IDisposable
    {
        private readonly IMemoryCache memoryCache;
        private readonly IDatabase redisDb;
        private readonly ISubscriber subscriber;
        private readonly ILogger<EntityCacheService> logger;

        private const string INVALIDATION_CHANNEL = "entity-cache-invalidation";
        private const string CLEAR_ALL_MESSAGES = "__CLEAR_ALL__";
        private bool disposed;

        private readonly HashSet<string> _memoryKeys = new();

        public EntityCacheService(IMemoryCache memoryCache, IConnectionMultiplexer redisMultiplexer, ILogger<EntityCacheService> logger)
        {
            this.memoryCache = memoryCache;
            redisDb = redisMultiplexer.GetDatabase();
            this.logger = logger;

            subscriber = redisMultiplexer.GetSubscriber();
            subscriber.Subscribe(INVALIDATION_CHANNEL, (channel, message) =>
            {
                var msg = message.ToString();
                if (msg == CLEAR_ALL_MESSAGES)
                    ClearAllMemoryCache();
                else
                    RemoveFromMemory(msg);
            });
        }

        private void TrackMemoryKey(string key)
        {
            lock (_memoryKeys)
            {
                _memoryKeys.Add(key);
            }
        }

        private void RemoveFromMemory(string key)
        {
            memoryCache.Remove(key);
            lock (_memoryKeys)
            {
                _memoryKeys.Remove(key);
            }
        }

        private void ClearAllMemoryCache()
        {
            lock (_memoryKeys)
            {
                foreach (var key in _memoryKeys)
                {
                    memoryCache.Remove(key);
                }
                _memoryKeys.Clear();
            }
            logger.LogInformation("Memory cache cleared completely.");
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? memoryExpiration = null, TimeSpan? redisExpiration = null)
        {
            if (memoryCache.TryGetValue<T>(key, out var memoryData))
            {
                logger.LogInformation("Cache hit: Memory | Key: {Key}", key);
                return memoryData;
            }

            var redisValue = await redisDb.StringGetAsync(key);
            if (redisValue.HasValue)
            {
                var redisData = JsonSerializer.Deserialize<T>(redisValue)!;
                logger.LogInformation("Cache hit: Redis | Key: {Key}", key);
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = memoryExpiration ?? TimeSpan.FromMinutes(1),
                    Size = 1
                };
                memoryCache.Set(key, redisData, options);
                TrackMemoryKey(key);
                return redisData;
            }

            logger.LogInformation("Cache miss | Key: {Key} — fetching from DB", key);
            var dbData = await factory();
            if (dbData != null)
            {
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = memoryExpiration ?? TimeSpan.FromMinutes(1),
                    Size = 1
                };
                memoryCache.Set(key, dbData, options);
                TrackMemoryKey(key);
                await redisDb.StringSetAsync(key, JsonSerializer.Serialize(dbData), redisExpiration ?? TimeSpan.FromMinutes(5));
            }

            return dbData;
        }


        public async Task SetAsync<T>(string key, T value, TimeSpan? memoryExpiration = null, TimeSpan? redisExpiration = null)
        {
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = memoryExpiration ?? TimeSpan.FromMinutes(1),
                Size = 1
            };
            memoryCache.Set(key, value, options);
            TrackMemoryKey(key);
            await redisDb.StringSetAsync(key, JsonSerializer.Serialize(value), redisExpiration ?? TimeSpan.FromMinutes(5));
            await subscriber.PublishAsync(INVALIDATION_CHANNEL, key);
        }

        public async Task RemoveAsync(string key)
        {
            RemoveFromMemory(key);
            await redisDb.KeyDeleteAsync(key);
            await subscriber.PublishAsync(INVALIDATION_CHANNEL, key);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            var server = redisDb.Multiplexer.GetServer(redisDb.Multiplexer.GetEndPoints()[0]);
            foreach (var key in server.Keys(pattern: pattern + "*"))
            {
                await redisDb.KeyDeleteAsync(key);
            }

            await subscriber.PublishAsync(INVALIDATION_CHANNEL, CLEAR_ALL_MESSAGES);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                subscriber?.Unsubscribe(INVALIDATION_CHANNEL);
                disposed = true;
            }
        }
    }
}