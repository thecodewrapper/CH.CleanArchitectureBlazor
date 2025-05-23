using System;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Core.Application
{
    /// <summary>
    /// Interface for a caching service that provides methods to interact with a cache store.
    /// Supports synchronous and asynchronous operations for getting, setting, checking existence, and removing cache entries.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets the value from cache. If it exists, returns the value. If not, it evaluates the result, caches it for the duration of <paramref name="durationSecs"/> and returns the <paramref name="evaluation"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="evaluation"></param>
        /// <param name="durationSecs"></param>
        /// <returns></returns>
        T GetSet<T>(string key, Func<T> evaluation, int durationSecs);
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key);
        void Set<T>(string key, T value);
        Task SetAsync<T>(string key, T value);
        void Set<T>(string key, T value, int durationSecs);
        Task SetAsync<T>(string key, T value, int durationSecs);
        void Set<T>(string key, T value, DateTime expiration);
        Task SetAsync<T>(string key, T value, DateTime expiration);
        bool Exists(string key);
        Task<bool> ExistsAsync(string key);
        void Remove(string key);
        Task RemoveAsync(string key);
    }
}
