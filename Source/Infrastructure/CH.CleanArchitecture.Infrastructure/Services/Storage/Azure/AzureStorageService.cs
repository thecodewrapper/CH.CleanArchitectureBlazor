using System;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using CH.CleanArchitecture.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class AzureStorageService
    {
        private readonly ILogger<AzureStorageService> _logger;
        private readonly StorageOptions _storageOptions;
        private readonly string _azureStorageAccountName;
        private readonly string _serviceEndpointFormat;

        public AzureStorageService(ILogger<AzureStorageService> logger, IOptions<StorageOptions> storageOptions) {
            _logger = logger;
            _storageOptions = storageOptions.Value;
            _azureStorageAccountName = _storageOptions.Azure.StorageAccountName;
            _serviceEndpointFormat = _storageOptions.Azure.ServiceEndpointFormat;
        }

        public BlobServiceClient GetBlobServiceClient() {
            bool usePasswordlessAuthentication = _storageOptions.Azure.UsePasswordlessAuthentication;

            if (usePasswordlessAuthentication) {
                // https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme?view=azure-dotnet#defaultazurecredential
                return new BlobServiceClient(new Uri(GetServiceEndpoint()), new DefaultAzureCredential());
            }
            else {
                string connString = _storageOptions.Azure.ConnectionString;
                return new BlobServiceClient(connString);
            }
        }

        public BlobServiceClient GetBlobServiceClientWithSharedKey() {
            string azureStorageAccountKey = _storageOptions.Azure.StorageAccountKey;

            //https://learn.microsoft.com/en-us/azure/storage/blobs/sas-service-create-dotnet?tabs=container
            StorageSharedKeyCredential storageSharedKeyCredential = new(_azureStorageAccountName, azureStorageAccountKey);

            _logger.LogDebug($"Retrieving blob client with shared key for storage account {_azureStorageAccountName}");
            return new BlobServiceClient(new Uri(GetServiceEndpoint()), storageSharedKeyCredential);
        }

        public BlobContainerClient GetBlobContainerClient(BlobServiceClient blobServiceClient, string containerName) {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName.ToLower()); // In Azure Storage, container names must be lower-case only
            return containerClient;
        }

        public string GetServiceEndpoint() {
            return string.Format(_serviceEndpointFormat, _azureStorageAccountName);
        }
    }
}
