namespace CH.CleanArchitecture.Infrastructure.Options
{
    public class EmailSenderOptions
    {
        /// <summary>
        /// Indicates whether to use SendGrid as an email sender
        /// </summary>
        public bool UseSendGrid { get; set; } = false;
        public bool UseAzureCommunicationServices { get; set; } = false;

        public AzureCommunicationServicesOptions Azure { get; set; } = new();
    }

    public class AzureCommunicationServicesOptions
    {
        /// <summary>
        /// The connection string for Azure Communication Services
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
