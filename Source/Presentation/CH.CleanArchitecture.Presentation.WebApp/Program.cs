using CH.CleanArchitecture.Infrastructure;
using CH.CleanArchitecture.Infrastructure.Services;
using Serilog;

namespace CH.CleanArchitecture.Presentation.WebApp
{
    public class Program
    {
        private static IConfigurationRoot _configurationRoot;

        public static void Main(string[] args) {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configurationBuilder = ConfigurationBuilderHelper.GetConfiguration(environment, null);
            _configurationRoot = configurationBuilder.Build();

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_configurationRoot).CreateLogger();
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try {
                    var dbInitializer = services.GetRequiredService<IDbInitializerService>();
                    logger.LogInformation($"Running database migration/seed");
                    dbInitializer.Migrate();
                    dbInitializer.Seed();
                }
                catch (Exception ex) {
                    logger.LogError(ex, "An error occurred while running database migration.");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}