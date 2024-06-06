using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace CH.CleanArchitecture.Presentation.EmailTemplates.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHtmlRenderingServices(this IServiceCollection services) {
            services.AddScoped<HtmlRenderer>();
            services.AddScoped<IComponentHtmlRenderer, BlazorHtmlRenderer>();
            services.AddScoped<IEmailViewModelService, EmailViewModelService>();

            return services;
        }
    }
}