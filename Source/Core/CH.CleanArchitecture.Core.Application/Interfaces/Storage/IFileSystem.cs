using System.IO;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IFileSystem
    {
        bool FileExists(string path);
        Stream OpenReadFile(string path);
        void DeleteFile(string path);
        void CreateFile(string path, byte[] contents);
        Task CreateFileAsync(string path, Stream contents);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
    }
}
