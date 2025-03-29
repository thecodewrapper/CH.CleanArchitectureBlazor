using System.IO;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IFileSystem
    {
        bool Exists(string path);
        Stream OpenRead(string path);
        void Delete(string path);
    }
}
