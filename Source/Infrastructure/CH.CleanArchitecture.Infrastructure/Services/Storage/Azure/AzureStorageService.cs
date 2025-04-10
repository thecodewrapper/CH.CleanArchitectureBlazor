using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.Constants;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class AzureStorageService
    {
        private readonly IApplicationConfigurationService _appConfigService;

        public AzureStorageService(IApplicationConfigurationService appConfigService) {
            _appConfigService = appConfigService;
        }

        public BlobServiceClient GetBlobServiceClient() {
            bool usePasswordlessAuthentication = _appConfigService.GetValueBool(AppConfigKeys.AZURE.STORAGE_USE_PASSWORDLESS_AUTHENTICATION).Unwrap();

            if (usePasswordlessAuthentication) {
                string azureStorageAccountName = _appConfigService.GetValue(AppConfigKeys.AZURE.STORAGE_ACCOUNT_NAME).Unwrap();
                // https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme?view=azure-dotnet#defaultazurecredential
                return new BlobServiceClient(new Uri(GetServiceEndpoint(azureStorageAccountName)), new DefaultAzureCredential());
            }
            else {
                string connString = _appConfigService.GetValue(AppConfigKeys.AZURE.STORAGE_CONNECTION_STRING).Unwrap();
                return new BlobServiceClient(connString);
            }
        }

        public BlobServiceClient GetBlobServiceClientWithSharedKey() {
            string azureStorageAccountName = _appConfigService.GetValue(AppConfigKeys.AZURE.STORAGE_ACCOUNT_NAME).Unwrap();
            string azureStorageAccountKey = _appConfigService.GetValue(AppConfigKeys.AZURE.STORAGE_ACCOUNT_KEY).Unwrap();
            StorageSharedKeyCredential storageSharedKeyCredential = new(azureStorageAccountName, azureStorageAccountKey);

            return new BlobServiceClient(new Uri(GetServiceEndpoint(azureStorageAccountName)), storageSharedKeyCredential);
        }

        public BlobContainerClient GetBlobContainerClient(BlobServiceClient blobServiceClient, string containerName) {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName.ToLower()); // In Azure Storage, container names must be lower-case only
            return containerClient;
        }

        private string GetServiceEndpoint(string accountName) {
            return $"https://{accountName}.blob.core.windows.net";
        }
    }
}
