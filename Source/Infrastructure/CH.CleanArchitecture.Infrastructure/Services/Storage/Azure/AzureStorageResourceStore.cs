using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CH.CleanArchitecture.Core.Application;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Implementation of <see cref="IResourceStore"/> using Azure Storage as the underlying store.
    /// This implementation retrieves relevant options from application configurations. You may change this to use <see cref="StorageOptions"/> along with any configuration required.
    /// </summary>
    internal class AzureStorageResourceStore : IResourceStore
    {
        #region Private Fields

        private readonly ILogger<AzureStorageResourceStore> _logger;
        private readonly AzureStorageService _azureStorageService;
        private readonly string _baseUri;

        public string ResourceProvider => "azure";

        #endregion Private Fields

        #region Public Constructors

        public AzureStorageResourceStore(ILogger<AzureStorageResourceStore> logger, AzureStorageService azureStorageService) {
            _logger = logger;
            _azureStorageService = azureStorageService;
            _baseUri = azureStorageService.GetServiceEndpoint();
        }

        #endregion Public Constructors

        #region Public Methods

        public string GetResourceURI(string path, string filename) {
            var (container, folder) = ParsePath(path);
            string blobPath = string.IsNullOrEmpty(folder) ? filename : $"{folder.TrimEnd('/')}/{filename}";
            return $"{_baseUri}/{container}/{blobPath}";
        }

        public async Task<bool> DeleteResourceAsync(string path, string filename) {
            try {
                var (container, folder) = ParsePath(path);
                string blobPath = string.IsNullOrEmpty(folder) ? filename : $"{folder.TrimEnd('/')}/{filename}";

                var blob = await GetBlobReferenceAsync(container, blobPath);
                var result = await blob.DeleteIfExistsAsync();

                _logger.LogDebug("Deleted blob {BlobPath} from container {Container}: {Result} in Azure Storage Blob", blobPath, container, result);
                return result;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to delete resource {Filename} from path {Path} in Azure Storage Blob", filename, path);
                throw;
            }
        }

        public async Task SaveResourceAsync(Stream stream, string path, bool isPublic, string filename) {
            try {
                var (container, folder) = ParsePath(path);
                string blobName = string.IsNullOrEmpty(folder) ? filename : $"{folder.TrimEnd('/')}/{filename}";

                var blob = await GetBlobReferenceAsync(container, blobName);
                await blob.UploadAsync(stream);

                _logger.LogDebug("Saved resource {Blob} to container {Container} (Stream length: {Length}) in Azure Storage Blob", blobName, container, stream.Length);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to save resource {Filename} to path {Path} in Azure Storage Blob", filename, path);
                throw;
            }
        }

        public async Task<string> SaveResourceAsync(Stream stream, string path, bool isPublic) {
            var randomId = Guid.NewGuid().ToString();
            await SaveResourceAsync(stream, path, isPublic, randomId);
            return randomId;
        }

        public async Task<Stream> DownloadResourceAsync(string path, string filename) {
            try {
                var (container, folder) = ParsePath(path);
                string blobPath = string.IsNullOrEmpty(folder) ? filename : $"{folder.TrimEnd('/')}/{filename}";

                var blob = await GetBlobReferenceAsync(container, blobPath);
                var response = await blob.DownloadContentAsync();

                _logger.LogInformation("Downloaded blob {BlobPath} from container {Container} from Azure Storage Blob", blobPath, container);
                return response.Value.Content.ToStream();
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to download resource {Filename} from path {Path} in Azure Storage Blob", filename, path);
                throw;
            }
        }

        public async Task CreateResourceFolderAsync(string containerName) {
            try {
                var blobServiceClient = _azureStorageService.GetBlobServiceClient();
                var containerClient = _azureStorageService.GetBlobContainerClient(blobServiceClient, containerName);
                await containerClient.CreateIfNotExistsAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Failed to create resource folder '{containerName}' in Azure Storage Blob.");
                throw;
            }
        }

        public async Task<List<string>> ListResourcesAsync(string path) {
            try {
                var (container, prefix) = ParsePath(path);
                if (!string.IsNullOrWhiteSpace(prefix))
                    prefix = prefix.TrimEnd('/') + "/";

                var blobServiceClient = _azureStorageService.GetBlobServiceClient();
                var containerClient = _azureStorageService.GetBlobContainerClient(blobServiceClient, container);

                var blobNames = new List<string>();
                await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix)) {
                    blobNames.Add(blobItem.Name);
                }

                _logger.LogInformation("Listed {Count} resources in container {Container} under prefix '{Prefix}'", blobNames.Count, container, prefix);
                return blobNames;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to list resources under path '{Path}'", path);
                throw;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private async ValueTask<BlobClient> GetBlobReferenceAsync(string containerName, string resourceId, bool createIfNotExists = false) {
            var blobServiceClient = _azureStorageService.GetBlobServiceClient();
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName.ToLower()); // In Azure Storage, container names must be lower-case only

            if (createIfNotExists) {
                await containerClient.CreateIfNotExistsAsync();
            }

            var blobClient = containerClient.GetBlobClient(resourceId);
            return blobClient;
        }

        private (string container, string blobPath) ParsePath(string input) {
            // Normalize slashes
            input = input.Replace('\\', '/').Trim('/');

            var slashIndex = input.IndexOf('/');
            if (slashIndex == -1)
                return (input.ToLowerInvariant(), string.Empty);

            var container = input.Substring(0, slashIndex).ToLowerInvariant();
            var blobPath = input.Substring(slashIndex + 1);

            return (container, blobPath);
        }

        #endregion Private Methods
    }
}
