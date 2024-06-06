using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Presentation.WebApp.Services
{
    public class HostUrlProvider : IHostUrlProvider
    {
        private readonly IConfiguration _configuration;

        public HostUrlProvider(IConfiguration configuration) {
            _configuration = configuration;
        }

        public Uri GetHostUrl() {
            string hostUrl = _configuration.GetValue<string>("Application:HostUrl");
            return new Uri(hostUrl);
        }
    }
}
