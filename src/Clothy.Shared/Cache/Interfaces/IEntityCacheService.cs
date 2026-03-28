using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Cache.Interfaces
{
    public interface IEntityCacheService
    {
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? memoryExpiration = null, TimeSpan? redisExpiration = null);
        Task SetAsync<T>(string key, T value, TimeSpan? memoryExpiration = null, TimeSpan? redisExpiration = null);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
    }
}
