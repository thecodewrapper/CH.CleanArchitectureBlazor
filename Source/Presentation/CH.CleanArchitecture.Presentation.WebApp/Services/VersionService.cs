namespace CH.CleanArchitecture.Presentation.WebApp.Services
{
    public class VersionService
    {
        private static string _currentVersion;
        public VersionService(IConfiguration configuration) {
            _currentVersion = $"v1.0.{configuration["VERSION"] ?? "0"}";
        }

        public string GetCurrentVersion() {
            return _currentVersion;
        }
    }
}
