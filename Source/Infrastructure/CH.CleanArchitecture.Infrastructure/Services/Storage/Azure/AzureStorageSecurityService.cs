using System;
using System.Threading.Tasks;
using Azure.Storage.Sas;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Implements the IResourceStoreSecurityService interface for Azure Storage. Used to create SAS tokens for accessing resources in Azure Storage.
    /// </summary>
    internal class AzureStorageSecurityService : IResourceStoreSecurityService
    {
        private readonly AzureStorageService _azureStorageService;

        public AzureStorageSecurityService(AzureStorageService azureStorageService) {
            _azureStorageService = azureStorageService;
        }

        public async Task<string> CreateReadOnlySecurityTokenAsync(string resourceName, string resourceType, DateTimeOffset expirationDate) {
            var blobServiceClient = _azureStorageService.GetBlobServiceClientWithSharedKey();
            var blobContainerClient = _azureStorageService.GetBlobContainerClient(blobServiceClient, resourceName);

            if (blobContainerClient.CanGenerateSasUri) {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = resourceName.ToLower(),
                    Resource = resourceType
                };

                sasBuilder.ExpiresOn = expirationDate;
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

                Uri sasURI = blobContainerClient.GenerateSasUri(sasBuilder);

                return sasURI.Query.ToString();
            }
            else {
                throw new InvalidOperationException("Cannot generate SAS URI for the specified blob container.");
            }
        }
    }
}
