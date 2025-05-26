using CH.CleanArchitecture.Infrastructure.Options;
using Microsoft.Extensions.Configuration;

namespace CH.CleanArchitecture.Infrastructure
{
    public static class OptionsHelper
    {
        public static StorageOptions GetStorageOptions(IConfiguration configuration) {
            StorageOptions storageOptions = new StorageOptions();
            configuration.GetSection("Storage").Bind(storageOptions);

            return storageOptions;
        }

        public static EmailSenderOptions GetEmailSenderOptions(IConfiguration configuration) {
            EmailSenderOptions emailSenderOptions = new EmailSenderOptions();
            configuration.GetSection("EmailSender").Bind(emailSenderOptions);

            return emailSenderOptions;
        }

        public static SMSSenderOptions GetSMSSenderOptions(IConfiguration configuration) {
            SMSSenderOptions smsSenderOptions = new SMSSenderOptions();
            configuration.GetSection("SMSSender").Bind(smsSenderOptions);

            return smsSenderOptions;
        }
    }
}
