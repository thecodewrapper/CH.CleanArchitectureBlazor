using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.Constants;
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

        #endregion Private Fields

        #region Public Constructors

        public AzureStorageResourceStore(ILogger<AzureStorageResourceStore> logger, AzureStorageService azureStorageService) {
            _logger = logger;
            _azureStorageService = azureStorageService;
        }

        #endregion Public Constructors

        #region Public Methods

        public string GetResourceURI(string containerName, string resourceId) {
            return $"{containerName.ToLower()}/{resourceId}";
        }

        public async Task<bool> DeleteResourceAsync(string containerName, string resourceId) {
            try {
                var blob = GetBlobReference(containerName, resourceId);
                _logger.LogDebug($"Attempting to delete resource {resourceId} from Azure Storage Blob (Container: {containerName}).");
                return await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Failed to delete resource {resourceId} from Azure Storage Blob (Container: {containerName}).");
                throw;
            }
        }

        public async Task SaveResourceAsync(Stream stream, string containerName, bool isPublic, string resourceId) {
            try {
                var blob = GetBlobReference(containerName, resourceId);
                await blob.UploadAsync(stream);
                _logger.LogDebug($"Saved resource {resourceId} successfully to Azure Storage Blob (Container: {containerName}, Stream length: {stream.Length})");
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Failed to save resource {resourceId} to Azure Storage Blob (Container: {containerName}, Stream length: {stream.Length})");
                throw;
            }
        }

        public async Task<string> SaveResourceAsync(Stream imageStream, string containerName, bool isPublic) {
            string resourceId = Guid.NewGuid().ToString(); //generating a random resource id
            await SaveResourceAsync(imageStream, containerName, isPublic, resourceId);
            return resourceId;
        }

        public async Task<Stream> DownloadResourceAsync(string containerName, string resourceId) {
            try {
                _logger.LogInformation($"Downloading resource '{containerName.ToLower()}'/{resourceId}");

                var blob = GetBlobReference(containerName, resourceId);
                MemoryStream memoryStream = new MemoryStream();
                await blob.DownloadToAsync(memoryStream);

                return memoryStream;
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Failed to download resource {resourceId} from Azure Storage Blob (Container: {containerName}");
                throw;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private BlobClient GetBlobReference(string containerName, string resourceId) {
            var blobServiceClient = _azureStorageService.GetBlobServiceClient();
            var container = blobServiceClient.GetBlobContainerClient(containerName.ToLower());
            var blob = container.GetBlobClient(resourceId);
            return blob;
        }

        #endregion Private Methods
    }
}
