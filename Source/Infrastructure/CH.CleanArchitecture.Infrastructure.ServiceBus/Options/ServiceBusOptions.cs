namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    public class ServiceBusOptions
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
    }
}
