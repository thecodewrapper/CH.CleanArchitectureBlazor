using System.IO;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class FileSystem : IFileSystem
    {
        public bool FileExists(string path) => File.Exists(path);
        public Stream OpenReadFile(string path) => File.OpenRead(path);
        public void DeleteFile(string path) => File.Delete(path);
        public void CreateFile(string path, byte[] contents) => File.WriteAllBytes(path, contents);
        public async Task CreateFileAsync(string path, Stream contents) {
            await using FileStream fs = new(path, FileMode.Create);
            await contents.CopyToAsync(fs);
        }

        public bool DirectoryExists(string path) => Directory.Exists(path);
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
        public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);
    }
}
