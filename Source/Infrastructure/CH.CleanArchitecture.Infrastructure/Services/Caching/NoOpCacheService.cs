using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// No operation cache service. This effectively disables caching when used
    /// </summary>
    public class NoOpCacheService : ICacheService
    {
        public bool Exists(string key) {
            return false;
        }

        public Task<bool> ExistsAsync(string key) {
            return Task.FromResult(false);
        }

        public T Get<T>(string key) {
            return default;
        }

        public Task<T> GetAsync<T>(string key) {
            return default;
        }

        public T GetSet<T>(string key, Func<T> evaluation, int durationSecs) {
            return evaluation();
        }

        public void Remove(string key) {
            //do nothing
        }

        public Task RemoveAsync(string key) {
            return Task.CompletedTask;
        }

        public void Set<T>(string key, T value) {
            //do nothing
        }

        public void Set<T>(string key, T value, int durationSecs) {
            //do nothing
        }

        public void Set<T>(string key, T value, DateTime expiration) {
            //do nothing
        }

        public Task SetAsync<T>(string key, T value) {
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, int durationSecs) {
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, DateTime expiration) {
            return Task.CompletedTask;
        }
    }
}
