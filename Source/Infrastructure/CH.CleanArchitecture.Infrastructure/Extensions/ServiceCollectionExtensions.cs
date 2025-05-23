using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.Extensions.ExpressionMapping;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Infrastructure.DbContexts;
using CH.CleanArchitecture.Infrastructure.DbContexts.Factories;
using CH.CleanArchitecture.Infrastructure.Factories;
using CH.CleanArchitecture.Infrastructure.Handlers.Queries;
using CH.CleanArchitecture.Infrastructure.Mappings;
using CH.CleanArchitecture.Infrastructure.Models;
using CH.CleanArchitecture.Infrastructure.Options;
using CH.CleanArchitecture.Infrastructure.Repositories;
using CH.CleanArchitecture.Infrastructure.ServiceBus;
using CH.CleanArchitecture.Infrastructure.Services;
using CH.CleanArchitecture.Presentation.EmailTemplates.Extensions;
using CH.Data.Abstractions;
using CH.EventStore.EntityFramework.Extensions;
using Hangfire;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CH.CleanArchitecture.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration) {
            services.AddDatabasePersistence(configuration);
            services.AddDbInitializer();
            services.AddRepositories();
            services.AddIdentity();
            services.AddEventSourcing(configuration);
            services.AddMapping();

            services.AddSharedServices();
            services.AddLocalizationServices();
            services.AddStorageServices(configuration);
            services.AddCommunicationServices(configuration);
            services.AddCryptoServices();
            services.AddAuthServices();
            services.AddScheduledJobs(configuration);
            services.AddMessaging(configuration);
            services.AddCaching();
        }

        public static void AddServiceBus(this IServiceCollection services, Action<ServiceBusBuilder> configure) {
            var builder = new ServiceBusBuilder(services);
            configure(builder);

            builder.Build();
        }

        private static IServiceCollection AddCaching(this IServiceCollection services) {
            services.AddScoped<ICacheKeyGenerator, CacheKeyGenerator>();

            services.AddMemoryCache();
            services.AddScoped<ICacheService, InMemoryCacheService>();

            return services;
        }

        private static void AddMessaging(this IServiceCollection services, IConfiguration configuration) {
            var serviceBusOptions = services.AddServiceBusOptions(configuration);
            var handlerAssemblies = new List<Assembly>
            {
                typeof(CreateUserCommandHandler).Assembly,
                typeof(GetAllUsersQueryHandler).Assembly
            };

            services.AddServiceBus(builder =>
            {
                builder.UseMediator(o =>
                {
                    o.WithAssemblies(handlerAssemblies.ToArray());
                });

                if (serviceBusOptions.Enabled) {
                    builder.UseServiceBus(serviceBusOptions.Provider, serviceBusOptions.HostUrl, o =>
                    {
                        //o.WithAssemblies([typeof(CreateUserCommand).Assembly]);
                        //o.WithProduce(typeof(CreateUserCommand));
                        //o.withconsume(typeof(CreateUserCommand));
                    });
                }
            });
        }

        private static void AddEventSourcing(this IServiceCollection services, IConfiguration configuration) {
            services.AddEventStoreEFCore((o) =>
            {
                o.UseInMemoryDatabase = configuration.GetValue<bool>("UseInMemoryDatabase");
                o.ConnectionStringSQL = configuration.GetConnectionString("ApplicationConnection");
            });
            services.AddTransient(typeof(IESRepository<,>), typeof(ESRepository<,>));
        }

        private static void AddDatabasePersistence(this IServiceCollection services, IConfiguration configuration) {
            if (configuration.GetValue<bool>("UseInMemoryDatabase")) {
                services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase("IdentityDb"));
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("ApplicationDb"));
            }
            else {
                services.AddIdentityDbContextFactory(options => options.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));
                services.AddApplicationDbContextFactory(options => options.UseSqlServer(configuration.GetConnectionString("ApplicationConnection")));
            }

            services.AddTransient<IIdentityContext, DefaultIdentityContext>();
        }

        private static void AddDbInitializer(this IServiceCollection services) {
            services.AddTransient<IDbInitializerService, DbInitializerService>();
        }

        private static void AddRepositories(this IServiceCollection services) {
            services.AddTransient(typeof(IEntityRepository<,>), typeof(DataEntityRepository<,>));
            services.AddTransient<IOrderRepository, OrderRepository>();
        }

        private static void AddMapping(this IServiceCollection services) {
            services.AddAutoMapper(config =>
            {
                config.AddExpressionMapping();
                config.AddProfile<AppProfile>();
                config.AddProfile<EventProfile>();
                config.AddProfile<UserProfile>();
                config.AddProfile<OrderProfile>();
            });
        }

        private static void AddScheduledJobs(this IServiceCollection services, IConfiguration configuration) {
            services.AddHangfire(x => x.UseSqlServerStorage(configuration.GetConnectionString("ApplicationConnection")));
            services.AddHangfireServer();
            services.AddTransient<IScheduledJobService, ScheduledJobService>();
        }

        private static void AddAuthServices(this IServiceCollection services) {
            services.AddTransient<IUserAuthenticationService, UserAuthenticationService>();
            services.AddTransient<IApplicationUserService, ApplicationUserService>();
        }

        private static void AddSharedServices(this IServiceCollection services) {

            services.AddTransient<IApplicationConfigurationService, ApplicationConfigurationService>();
            services.AddTransient<IAuditHistoryService, AuditHistoryService>();

            services.AddHtmlRenderingServices();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<INotificationContentService, NotificationContentService>();
        }

        private static void AddLocalizationServices(this IServiceCollection services) {
            services.AddTransient<ILocalizationService, LocalizationService>();
            services.AddTransient<ILocalizationKeyProvider, LocalizationKeyProvider>();
        }

        /// <summary>
        /// Adds storage services. Conditionally add the required storage services here. See <see cref="IResourceStore"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddStorageServices(this IServiceCollection services, IConfiguration configuration) {
            var options = GetStorageOptions(configuration);
            services.Configure<StorageOptions>(c =>
            {
                c.StorageProvider = options.StorageProvider;
                c.BasePath = options.BasePath;
                c.Azure = options.Azure;
                c.AWS = options.AWS;
            });

            if (string.IsNullOrEmpty(options.StorageProvider)) {
                return;
            }

            services.AddTransient<IResourceStoreSecurityService, NoOpStorageSecurityService>();

            switch (options.StorageProvider.ToLower()) {
                case "azure":
                    services.AddTransient<AzureStorageService>();
                    services.AddTransient<IResourceStore, AzureStorageResourceStore>();
                    services.AddTransient<IResourceStoreSecurityService, AzureStorageSecurityService>();
                    break;
                case "aws":
                    services.AddTransient<IResourceStore, AWSS3ResourceStore>();
                    break;
                case "local":
                    services.AddTransient<IResourceStore>(provider => new LocalFileResourceStore(options.BasePath));
                    break;
                default:
                    throw new ArgumentException("Invalid storage provider specified.");
            }

            services.AddTransient<IResourcesService, ResourcesService>();
            services.AddSingleton<IFileSystem, FileSystem>();
        }

        private static void AddCommunicationServices(this IServiceCollection services, IConfiguration configuration) {
            var emailSenderOptions = GetEmailSenderOptions(configuration);
            if (emailSenderOptions.UseSendGrid) {
                services.AddTransient<IEmailService, EmailSendGridService>();
            }
            else {
                services.AddTransient<IEmailService, EmailSMTPService>();
            }

            services.AddTransient<ISMSService, SMSService>();
        }

        private static void AddCryptoServices(this IServiceCollection services) {
            services.AddTransient<IJWTService, JWTService>();
            services.AddTransient<IUrlTokenService, UrlTokenService>();
            services.AddTransient<IPasswordGeneratorService, PasswordGeneratorIdentityService>();
        }

        private static void AddIdentity(this IServiceCollection services) {
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddRoleManager<RoleManager<ApplicationRole>>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders()
                .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

            var passwordOptions = new PasswordOptions()
            {
                RequireDigit = false, //in accordance with ASVS 4.0
                RequiredLength = 10, //in accordance with ASVS 4.0
                RequireUppercase = false, //in accordance with ASVS 4.0
                RequireLowercase = false //in accordance with ASVS 4.0
            };

            services.AddScoped(a => passwordOptions);

            services.Configure<IdentityOptions>(options =>
            {
                options.Password = passwordOptions;

            });
        }

        private static IServiceCollection AddApplicationDbContextFactory(this IServiceCollection services, Action<DbContextOptionsBuilder>? options) {
            services.AddDbContext<ApplicationDbContext>(options);
            services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(serviceProvider =>
            {
                using var scope = serviceProvider.CreateScope();
                var authenticatedUserService = scope.ServiceProvider.GetRequiredService<IIdentityContext>();

                return new ApplicationDbContextFactory(options, authenticatedUserService);
            });

            return services;
        }

        private static IServiceCollection AddIdentityDbContextFactory(this IServiceCollection services, Action<DbContextOptionsBuilder>? options) {
            services.AddDbContext<IdentityDbContext>(options);
            services.AddSingleton<IDbContextFactory<IdentityDbContext>>(serviceProvider =>
            {
                using var scope = serviceProvider.CreateScope();
                var authenticatedUserService = scope.ServiceProvider.GetRequiredService<IIdentityContext>();

                return new IdentityDbContextFactory(options, authenticatedUserService);
            });

            return services;
        }

        private static StorageOptions GetStorageOptions(IConfiguration configuration) {
            StorageOptions storageOptions = new StorageOptions();
            configuration.GetSection("Storage").Bind(storageOptions);

            return storageOptions;
        }

        private static EmailSenderOptions GetEmailSenderOptions(IConfiguration configuration) {
            EmailSenderOptions emailSenderOptions = new EmailSenderOptions();
            configuration.GetSection("EmailSender").Bind(emailSenderOptions);

            return emailSenderOptions;
        }
    }
}