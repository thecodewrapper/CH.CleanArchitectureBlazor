using Microsoft.Extensions.Configuration;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    internal class ReplyQueueResolver
    {
        private readonly IConfiguration _configuration;

        public ReplyQueueResolver(IConfiguration configuration) {
            _configuration = configuration;
        }

        public string GetReplyQueueName() {
            var appName = _configuration["Application:Name"] ?? "app";
            return $"{appName}.replies";
        }
    }
}
