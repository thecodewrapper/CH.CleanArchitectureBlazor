using System.IO;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    public class FileSystem : IFileSystem
    {
        public bool Exists(string path) => File.Exists(path);
        public Stream OpenRead(string path) => File.OpenRead(path);
        public void Delete(string path) => File.Delete(path);
    }
}
