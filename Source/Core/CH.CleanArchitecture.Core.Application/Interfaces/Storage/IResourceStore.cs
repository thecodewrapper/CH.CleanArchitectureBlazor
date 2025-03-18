using System.IO;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Core.Application
{
    /// <summary>
    /// Abstraction for a resource/file store
    /// </summary>
    public interface IResourceStore
    {
        Task SaveResourceAsync(Stream stream, string path, bool isPublic, string resourceId);
        Task<string> SaveResourceAsync(Stream stream, string path, bool isPublic);
        Task<Stream> DownloadResourceAsync(string containerName, string resourceId);
        Task<bool> DeleteResourceAsync(string containerName, string resourceId);
        string GetResourceURI(string folder, string resourceId);
    }
}
