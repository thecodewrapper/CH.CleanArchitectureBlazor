using AutoMapper;
using CH.CleanArchitecture.Infrastructure.ServiceBus;
using CH.CleanArchitecture.Infrastructure.Shared.Culture;
using CH.CleanArchitecture.Presentation.WebApp.Extensions;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.IdentityModel.Logging;
using MudBlazor.Services;

namespace CH.CleanArchitecture.Presentation.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) {
            Configuration = configuration;
            HostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment HostEnvironment { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();

            //BLAZOR
            services.AddRazorPages();
            services.AddServerSideBlazor()
                .AddCircuitOptions(options =>
            {
                if (HostEnvironment.IsDevelopment() || HostEnvironment.IsStaging()) {
                    options.DetailedErrors = true;
                }
                options.DisconnectedCircuitMaxRetained = 1;
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(1);
            });

            services.AddMudServices();

            //LOCALIZATION
            services.AddLocalization(options => options.ResourcesPath = "");

            //CACHING
            services.AddResponseCaching();

            //INFRASTRUCTURE
            services.AddInfrastructure(Configuration);

            //APPLICATION
            services.AddApplication();

            if (HostEnvironment.IsDevelopment()) {
                IdentityModelEventSource.ShowPII = true;
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration, IMapper mapper) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers.Add("Content-Security-Policy", "base-uri 'self';default-src 'self';img-src data: https:;object-src 'none';script-src 'self''unsafe-eval';style-src 'self';upgrade-insecure-requests;");
            //    await next();
            //});

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { app.ApplicationServices.GetService<IDashboardAuthorizationFilter>() }
            });

            var supportedCultures = new[] { "el", "en" };
            var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            localizationOptions.RequestCultureProviders.Insert(0, new UserProfileRequestCultureProvider());
            app.UseRequestLocalization(localizationOptions);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
