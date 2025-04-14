using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class LocalFileResourceStore : IResourceStore
    {
        private readonly string _baseDirectory;

        public string ResourceProvider => "local";

        public LocalFileResourceStore(string baseDirectory) {
            _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
            Directory.CreateDirectory(_baseDirectory); // Ensure base directory exists
        }

        public async Task<bool> DeleteResourceAsync(string folder, string filename) {
            string fullPath = GetFullPath(folder, filename);
            if (File.Exists(fullPath)) {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        public async Task<Stream> DownloadResourceAsync(string folder, string filename) {
            string fullPath = GetFullPath(folder, filename);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File not found", fullPath);

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public string GetResourceURI(string folder, string filename) {
            string fullPath = GetFullPath(folder, filename);
            return new Uri(fullPath).AbsoluteUri;
        }

        public async Task SaveResourceAsync(Stream stream, string path, bool isPublic, string filename) {
            string fullPath = GetFullPath(path, filename);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                await stream.CopyToAsync(fileStream);
            }
        }

        public async Task<string> SaveResourceAsync(Stream stream, string path, bool isPublic) {
            string filename = Guid.NewGuid().ToString(); // Generate a unique filename
            await SaveResourceAsync(stream, path, isPublic, filename);
            return filename;
        }

        public async Task<List<string>> ListResourcesAsync(string folder) {
            string fullPath = Path.Combine(_baseDirectory, folder);

            if (!Directory.Exists(fullPath)) {
                throw new DirectoryNotFoundException($"The folder '{folder}' does not exist.");
            }

            var files = Directory.GetFiles(fullPath);
            var fileNames = new List<string>();

            foreach (var file in files) {
                fileNames.Add(Path.GetFileName(file));
            }

            return await Task.FromResult(fileNames);
        }

        private string GetFullPath(string folder, string filename) {
            return Path.Combine(_baseDirectory, folder, filename);
        }
    }
}
