using Microsoft.Extensions.DependencyInjection;
namespace CH.CleanArchitecture.Core.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddIdentityContext(this IServiceCollection services) {
            services.AddScoped<IIdentityContext, IdentityContext>();
        }
    }
}
