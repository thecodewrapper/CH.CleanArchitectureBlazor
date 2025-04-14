using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Implementation of <see cref="IResourceStore"/> using AWS S3 as the underlying store.
    /// This implementation retrieves relevant options from application configurations. You may change this to use <see cref="StorageOptions"/> along with any configuration required.
    /// </summary>
    internal class AWSS3ResourceStore : IResourceStore
    {
        private readonly ILogger<AWSS3ResourceStore> _logger;
        private readonly StorageOptions _storageOptions;
        private string _bucketName;
        private RegionEndpoint _bucketRegion;

        public string ResourceProvider => "aws";

        internal AWSS3ResourceStore(ILogger<AWSS3ResourceStore> logger, IOptions<StorageOptions> storageOptions) {
            _logger = logger;
            _storageOptions = storageOptions.Value;
            _bucketName = _storageOptions.AWS.BucketName;
            _bucketRegion = RegionEndpoint.GetBySystemName(_storageOptions.AWS.Region);
        }

        public async Task<bool> DeleteResourceAsync(string folder, string resourceId) {
            try {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = GetS3ObjectKey(folder, resourceId)
                };

                _logger.LogInformation($"Attempting to delete resource {resourceId} from AWS S3 (Folder: {folder}).");

                AmazonS3Client client = GetAmazonS3Client();
                DeleteObjectResponse response = await client.DeleteObjectAsync(deleteObjectRequest);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Failed to delete resource {resourceId} from AWS S3 (Folder: {folder}).");
                throw;
            }
        }

        public async Task<Stream> DownloadResourceAsync(string folder, string resourceId) {
            try {
                _logger.LogInformation($"Downloading resource '{folder}'/{resourceId}");

                AmazonS3Client client = GetAmazonS3Client();
                GetObjectResponse response = await client.GetObjectAsync(_bucketName, GetS3ObjectKey(folder, resourceId));
                MemoryStream memoryStream = new MemoryStream();
                using Stream responseStream = response.ResponseStream;
                await responseStream.CopyToAsync(memoryStream);

                return memoryStream;
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Failed to download resource {resourceId} from AWS S3 (Folder: {folder}");
                throw;
            }
        }

        public string GetResourceURI(string folder, string resourceId) {
            string baseUrl = GetAmazonS3ServiceURL();
            string resourceURI = Path.Combine(baseUrl, GetS3ObjectKey(folder, resourceId));
            return resourceURI;
        }

        public async Task SaveResourceAsync(Stream stream, string folder, bool isPublic, string resourceId) {
            try {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = GetS3ObjectKey(folder, resourceId),
                    InputStream = stream
                };

                //comment out and modify the below to fit your needs on S3
                //https://docs.aws.amazon.com/AmazonS3/latest/userguide/about-object-ownership.html?icmpid=docs_amazons3_console
                //if (isPublic) {
                //    putRequest.CannedACL = S3CannedACL.PublicRead;
                //}
                //else {
                //    putRequest.CannedACL = S3CannedACL.BucketOwnerFullControl;
                //}

                AmazonS3Client client = GetAmazonS3Client();
                PutObjectResponse response = await client.PutObjectAsync(putRequest);
                _logger.LogInformation($"Saved resource {resourceId} successfully to AWS S3 (Folder: {folder}, Stream length: {stream.Length})");
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Failed to save resource {resourceId} to AWS S3 (Folder: {folder}, Stream length: {stream.Length})");
                throw;
            }
        }

        public async Task<string> SaveResourceAsync(Stream stream, string path, bool isPublic) {
            string resourceId = Guid.NewGuid().ToString(); //generating a random resource id
            await SaveResourceAsync(stream, path, isPublic, resourceId);
            return resourceId;
        }

        public async Task<List<string>> ListResourcesAsync(string folder) {
            try {
                _logger.LogInformation($"Listing resources in folder: {folder}");

                AmazonS3Client client = GetAmazonS3Client();
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = string.IsNullOrEmpty(folder) ? null : $"{folder}/",
                    MaxKeys = 100 // Adjust as needed
                };

                var resourceKeys = new List<string>();
                ListObjectsV2Response response;

                do {
                    response = await client.ListObjectsV2Async(request);
                    foreach (S3Object entry in response.S3Objects) {
                        string key = entry.Key;
                        if (!string.IsNullOrEmpty(folder)) {
                            key = key.Substring(folder.Length + 1); // Remove folder prefix
                        }
                        resourceKeys.Add(key);
                    }

                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                _logger.LogInformation($"Successfully listed {resourceKeys.Count} resources in folder: {folder}");
                return resourceKeys;
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Failed to list resources in folder: {folder}");
                throw;
            }
        }

        private AmazonS3Client GetAmazonS3Client() {
            string awsAccessKeyId = _storageOptions.AWS.AWSAccessKeyId;
            string awsSecretAccessKey = _storageOptions.AWS.AWSSecretAccessKey;

            AWSCredentials credentials = new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey);
            AmazonS3Config config = new AmazonS3Config()
            {
                ServiceURL = GetAmazonS3ServiceURL(),
                ForcePathStyle = true
            };

            return new AmazonS3Client(credentials, config);
        }

        private string GetAmazonS3ServiceURL() {
            string endpointFormat = _storageOptions.AWS.EndpointFormat;
            string baseUrl = string.Format(endpointFormat, _bucketName, _bucketRegion.SystemName);

            return baseUrl;
        }

        private static string GetS3ObjectKey(string folder, string resourceId) {
            if (string.IsNullOrEmpty(folder)) {
                return resourceId;
            }
            return $"{folder}/{resourceId}";
        }
    }
}
