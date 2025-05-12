using System.Collections.Concurrent;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Tests.Mocks
{
    internal class MockResourceStore : IResourceStore
    {
        private static ConcurrentDictionary<string, byte[]> _store = new();

        public string ResourceProvider => "mock";
        public Task<bool> DeleteResourceAsync(string folder, string filename) {
            var key = $"{folder}/{filename}";
            return Task.FromResult(_store.TryRemove(key, out _));
        }

        public Task<Stream> DownloadResourceAsync(string folder, string filename) {
            var key = $"{folder}/{filename}";
            if (_store.TryGetValue(key, out var data)) {
                return Task.FromResult<Stream>(new MemoryStream(data));
            }
            throw new FileNotFoundException("Resource not found");
        }

        public string GetResourceURI(string folder, string filename) {
            var key = $"{folder}/{filename}";
            return _store.ContainsKey(key) ? $"mock://{key}" : null;
        }

        public Task SaveResourceAsync(Stream stream, string path, bool isPublic, string filename) {
            var key = $"{path}/{filename}";
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            _store[key] = memoryStream.ToArray();
            return Task.CompletedTask;
        }

        public Task<string> SaveResourceAsync(Stream stream, string path, bool isPublic) {
            var key = $"{path}/{Guid.NewGuid()}";
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            _store[key] = memoryStream.ToArray();
            return Task.FromResult(key);
        }

        public Task<List<string>> ListResourcesAsync(string path) {
            var resources = _store.Keys
                .Where(key => key.StartsWith($"{path}/"))
                .ToList();
            return Task.FromResult(resources);
        }

        public Task CreateResourceFolderAsync(string folder) {
            return Task.CompletedTask;
        }
    }
}
