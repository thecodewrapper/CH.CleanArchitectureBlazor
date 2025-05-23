using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly int _defaultSlidingExpiration;

        public InMemoryCacheService(IMemoryCache cache, IApplicationConfigurationService appConfigService) {
            _cache = cache;
            _defaultSlidingExpiration = appConfigService.GetValueInt(AppConfigKeys.CACHE.SLIDING_EXPIRATION_TIME).Unwrap();
        }

        public T GetSet<T>(string key, Func<T> evaluation, int durationSecs) {
            if (!_cache.TryGetValue(key, out T value)) {
                value = evaluation();
                _cache.Set(key, value, TimeSpan.FromSeconds(durationSecs));
            }
            return value;
        }

        public T Get<T>(string key) {
            _cache.TryGetValue(key, out T value);
            return value;
        }

        public Task<T> GetAsync<T>(string key) {
            return Task.FromResult(Get<T>(key));
        }

        public void Set<T>(string key, T value) {
            _cache.Set(key, value, TimeSpan.FromSeconds(_defaultSlidingExpiration));
        }

        public Task SetAsync<T>(string key, T value) {
            Set(key, value);
            return Task.CompletedTask;
        }

        public void Set<T>(string key, T value, int durationSecs) {
            _cache.Set(key, value, TimeSpan.FromSeconds(durationSecs));
        }

        public Task SetAsync<T>(string key, T value, int durationSecs) {
            Set(key, value, durationSecs);
            return Task.CompletedTask;
        }

        public void Set<T>(string key, T value, DateTime expiration) {
            var duration = expiration - DateTime.UtcNow;
            if (duration <= TimeSpan.Zero)
                duration = TimeSpan.FromSeconds(1); // Prevent immediate expiration
            _cache.Set(key, value, duration);
        }

        public Task SetAsync<T>(string key, T value, DateTime expiration) {
            Set(key, value, expiration);
            return Task.CompletedTask;
        }

        public bool Exists(string key) {
            return _cache.TryGetValue(key, out _);
        }

        public Task<bool> ExistsAsync(string key) {
            return Task.FromResult(Exists(key));
        }

        public void Remove(string key) {
            _cache.Remove(key);
        }

        public Task RemoveAsync(string key) {
            Remove(key);
            return Task.CompletedTask;
        }
    }
}
