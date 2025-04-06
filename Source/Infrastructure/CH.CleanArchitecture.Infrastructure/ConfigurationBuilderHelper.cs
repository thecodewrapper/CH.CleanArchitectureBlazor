using System;
using System.IO;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace CH.CleanArchitecture.Infrastructure
{
    public static class ConfigurationBuilderHelper
    {
        public static IConfigurationBuilder GetConfiguration(string environment, IConfigurationBuilder configurationBuilder) {
            configurationBuilder ??= new ConfigurationBuilder();

            configurationBuilder = configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", true)
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();
            var azureAppConfigurationEndpoint = configuration.GetValue<string>("AzureAppConfigurationEndpoint");
            if (!string.IsNullOrEmpty(azureAppConfigurationEndpoint))
                configurationBuilder.AddAzureAppConfiguration(options => options.Connect(new Uri(azureAppConfigurationEndpoint), new ManagedIdentityCredential()));

            return configurationBuilder;
        }
    }
}
