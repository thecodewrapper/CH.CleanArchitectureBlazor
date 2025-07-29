namespace CH.CleanArchitecture.Infrastructure.Options
{
    public class StorageOptions
    {
        public string BasePath { get; set; }
        public string StorageProvider { get; set; }
        public AzureStorageOptions Azure { get; set; }
        public AWSS3Options AWS { get; set; }

        public class AzureStorageOptions
        {
            /// <summary>
            /// The Azure Storage account name.
            /// </summary>
            public string StorageAccountName { get; set; }

            /// <summary>
            /// Whether to use passwordless authentication (DefaultAzureCredential), or connection string, for connecting to Azure Storage.
            /// If this is true, the StorageAccountName needs to be set.
            /// </summary>
            public bool UsePasswordlessAuthentication { get; set; }

            /// <summary>
            /// Connection string for the Azure Storage account.
            /// </summary>
            public string ConnectionString { get; set; }

            /// <summary>
            /// The Azure Storage account key.
            /// </summary>
            public string StorageAccountKey { get; set; }

            /// <summary>
            /// The format of the Azure Storage service endpoint.
            /// </summary>
            public string ServiceEndpointFormat { get; set; }
        }

        public class AWSS3Options
        {
            /// <summary>
            /// The AWS S3 bucket name.
            /// </summary>
            public string BucketName { get; set; }

            /// <summary>
            /// The AWS S3 region. Use the system name of the region'. Example: 'eu-west-1'
            /// </summary>
            public string Region { get; set; }

            /// <summary>
            /// The string format of S3 public url endpoint. Use '{0}' as a placeholder for the bucket name and '{1}' as a placeholder for the region
            /// </summary>
            public string EndpointFormat { get; set; } //Example: "https://{0}.s3.{1}.amazonaws.com"

            /// <summary>
            /// The AWS access key id. Used for authenticating with AWS
            /// </summary>
            public string AWSAccessKeyId { get; set; }

            /// <summary>
            /// The AWS secret access key. Used for authenticating with AWS
            /// </summary>
            public string AWSSecretAccessKey { get; set; }
        }
    }
}
