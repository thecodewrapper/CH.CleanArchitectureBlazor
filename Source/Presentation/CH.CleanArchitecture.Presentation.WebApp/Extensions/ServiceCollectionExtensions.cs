﻿using AutoMapper.Extensions.ExpressionMapping;
using Blazored.Toast;
using FluentValidation;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Extensions;
using CH.CleanArchitecture.Infrastructure.Extensions;
using CH.CleanArchitecture.Infrastructure.Models;
using CH.CleanArchitecture.Presentation.Framework;
using CH.CleanArchitecture.Presentation.Framework.Interfaces;
using CH.CleanArchitecture.Presentation.Framework.Services;
using CH.CleanArchitecture.Presentation.Web.Services;
using CH.CleanArchitecture.Presentation.WebApp.Mappings;
using CH.CleanArchitecture.Presentation.WebApp.Services;
using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Presentation.WebApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Blazor/Web App related services here
        /// </summary>
        /// <param name="services"></param>
        public static void AddApplication(this IServiceCollection services) {
            services.AddApplicationLayer();

            services.AddApplicationCookie();
            services.AddHttpContextAccessor();

            //Configure Hangfire dashboard authorization
            services.AddApplicationAuthorization((options) => options.AddPolicy(WebFrameworkConstants.HANGFIRE_DASHBOARD_POLICY_NAME, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(RoleEnum.SuperAdmin.ToString());
            }));

            services.AddHangfireDashboardAuthorizationFilter();

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<UserAccountService>();

            services.AddBlazoredToast();
            services.AddTransient<IModalService, DefaultModalService>();
            services.AddTransient<IToastService, ToastService>();
            services.AddTransient<LoaderService>();
            services.AddTransient<DocumentsService>();
            services.AddAutoMapper(config =>
            {
                config.AddExpressionMapping();
                config.AddProfile<AppProfile>();
            });

            services.AddTransient<IAuthorizationStateProvider, AuthorizationStateProvider>();
            services.AddTransient<IIdentityProvider, IdentityProvider>();

            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
            services.AddTransient<IHostUrlProvider, HostUrlProvider>();
        }

        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
            services.AddInfrastructureLayer(configuration);
            services.AddValidatorsFromAssemblyContaining<Startup>();
        }

        private static void AddApplicationCookie(this IServiceCollection services) {
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.Name = "CH.CleanArchitecture.AUTH";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });
        }

        private static void AddHangfireDashboardAuthorizationFilter(this IServiceCollection services) {
            // Register the HangfireDashboardAuthorizationFilter using a factory method
            services.AddTransient<IDashboardAuthorizationFilter>(serviceProvider =>
            {
                var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                return new HangfireDashboardAuthorizationFilter(authorizationService, httpContextAccessor);
            });
        }
    }
}