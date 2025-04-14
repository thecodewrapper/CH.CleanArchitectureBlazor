using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Tests.Mocks
{
    internal class MockFileSystem : IFileSystem
    {
        private readonly Dictionary<string, MockFileData> _files = new Dictionary<string, MockFileData>();
        private readonly HashSet<string> _directories = new HashSet<string>();

        public bool FileExists(string path) {
            return _files.ContainsKey(path);
        }

        public Stream OpenReadFile(string path) {
            if (!_files.ContainsKey(path)) {
                throw new FileNotFoundException($"File not found: {path}");
            }

            return new MemoryStream(_files[path].Content);
        }

        public void DeleteFile(string path) {
            if (_files.ContainsKey(path)) {
                _files.Remove(path);
            }
        }

        public void CreateFile(string path, byte[] contents) {
            _files[path] = new MockFileData(contents);
        }

        public bool DirectoryExists(string path) {
            return _directories.Contains(path);
        }

        public void CreateDirectory(string path) {
            _directories.Add(path);
        }

        public async Task CreateFileAsync(string path, Stream contents) {
            using (var memoryStream = new MemoryStream()) {
                await contents.CopyToAsync(memoryStream);
                _files[path] = new MockFileData(memoryStream.ToArray());
            }
        }
    }

    public class MockFileData
    {
        public byte[] Content { get; }

        public MockFileData(string content) {
            Content = System.Text.Encoding.UTF8.GetBytes(content);
        }

        public MockFileData(byte[] content) {
            Content = content;
        }
    }
}
