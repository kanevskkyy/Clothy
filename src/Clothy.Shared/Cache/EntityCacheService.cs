﻿using Clothy.Shared.Cache.Interfaces;
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
        private IMemoryCache memoryCache;
        private IDatabase redisDb;
        private ISubscriber subscriber;
        private ILogger<EntityCacheService> logger;
        private HashSet<string> memoryKeys = new();

        private const string INVALIDATION_CHANNEL = "entity-cache-invalidation";
        private const string CLEAR_ALL_MESSAGES = "__CLEAR_ALL__";
        private bool disposed;

        private static JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = null, 
            WriteIndented = false,
            PropertyNameCaseInsensitive = true 
        };

        public EntityCacheService(IMemoryCache memoryCache, IConnectionMultiplexer redisMultiplexer, ILogger<EntityCacheService> logger)
        {
            this.memoryCache = memoryCache;
            redisDb = redisMultiplexer.GetDatabase();
            this.logger = logger;

            subscriber = redisMultiplexer.GetSubscriber();
    
            _ = subscriber.SubscribeAsync(INVALIDATION_CHANNEL, (channel, message) =>
            {
                string msg = message.ToString();
                if (msg == CLEAR_ALL_MESSAGES) ClearAllMemoryCache();
                else RemoveFromMemory(msg);
            });
        }

        private void TrackMemoryKey(string key)
        {
            lock (memoryKeys)
            {
                memoryKeys.Add(key);
            }
        }

        private void RemoveFromMemory(string key)
        {
            memoryCache.Remove(key);
            lock (memoryKeys)
            {
                memoryKeys.Remove(key);
            }
        }

        private void ClearAllMemoryCache()
        {
            lock (memoryKeys)
            {
                foreach (var key in memoryKeys)
                {
                    memoryCache.Remove(key);
                }
                memoryKeys.Clear();
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

            try
            {
                RedisValue redisValue = await redisDb.StringGetAsync(key);
                if (redisValue.HasValue)
                {
                    T redisData = JsonSerializer.Deserialize<T>(redisValue, JsonOptions)!;
                    MemoryCacheEntryOptions memoryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = memoryExpiration ?? TimeSpan.FromMinutes(1),
                        Size = 1
                    };
                    memoryCache.Set(key, redisData, memoryOptions);
                    TrackMemoryKey(key);
                    return redisData;
                }
            }
            catch (RedisConnectionException ex)
            {
                logger.LogWarning(ex, "Redis unavailable, fallback to DB | Key: {Key}", key);
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
                try
                {
                    await redisDb.StringSetAsync(key, JsonSerializer.Serialize(dbData), redisExpiration ?? TimeSpan.FromMinutes(5));
                }
                catch (RedisConnectionException ex)
                {
                    logger.LogWarning(ex, "Redis unavailable, failed to write | Key: {Key}", key);
                }
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
            await redisDb.StringSetAsync(key, JsonSerializer.Serialize(value, JsonOptions), redisExpiration ?? TimeSpan.FromMinutes(5));
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
            IServer server = redisDb.Multiplexer.GetServer(redisDb.Multiplexer.GetEndPoints()[0]);
            await foreach (var key in server.KeysAsync(pattern: pattern + "*"))
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