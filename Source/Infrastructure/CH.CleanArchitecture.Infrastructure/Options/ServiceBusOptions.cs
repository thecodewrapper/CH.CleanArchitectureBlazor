namespace CH.CleanArchitecture.Infrastructure.Options
{
    internal class ServiceBusOptions
    {
        /// <summary>
        /// Whether services bus should be enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The service bus provider (e.g. azure, aws)
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// The host url of the service bus
        /// </summary>
        public string HostUrl { get; set; }

        /// <summary>
        /// The input queue name
        /// </summary>
        public string InputQueueName { get; set; }
    }
}
