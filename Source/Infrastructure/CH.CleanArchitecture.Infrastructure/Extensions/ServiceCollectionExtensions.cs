using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ardalis.GuardClauses;
using AutoMapper.Extensions.ExpressionMapping;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Core.Application.Interfaces.Storage;
using CH.CleanArchitecture.Infrastructure.DbContexts;
using CH.CleanArchitecture.Infrastructure.DbContexts.Factories;
using CH.CleanArchitecture.Infrastructure.Factories;
using CH.CleanArchitecture.Infrastructure.Handlers.Queries;
using CH.CleanArchitecture.Infrastructure.Mappings;
using CH.CleanArchitecture.Infrastructure.Models;
using CH.CleanArchitecture.Infrastructure.Options;
using CH.CleanArchitecture.Infrastructure.Repositories;
using CH.CleanArchitecture.Infrastructure.Services;
using CH.CleanArchitecture.Infrastructure.Services.Storage;
using CH.CleanArchitecture.Presentation.EmailTemplates.Extensions;
using CH.Data.Abstractions;
using CH.EventStore.EntityFramework.Extensions;
using CH.Messaging.Abstractions;
using Hangfire;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CH.CleanArchitecture.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration) {
            services.AddDatabasePersistence(configuration);
            services.AddRepositories();
            services.AddIdentity();
            services.AddEventStoreEFCore((o) =>
            {
                o.UseInMemoryDatabase = configuration.GetValue<bool>("UseInMemoryDatabase");
                o.ConnectionStringSQL = configuration.GetConnectionString("ApplicationConnection");
            });
            services.AddMapping();

            services.AddSharedServices();
            services.AddLocalizationServices();
            services.AddStorageServices(configuration);
            services.AddCommunicationServices(configuration);
            services.AddCryptoServices();
            services.AddAuthServices();
            services.AddScheduledJobs(configuration);
            services.AddServiceBus(configuration);
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
            services.AddTransient<IDbInitializerService, DbInitializerService>();
            services.AddTransient<IIdentityContext, DefaultIdentityContext>();
        }

        private static void AddRepositories(this IServiceCollection services) {
            services.AddTransient(typeof(IEntityRepository<,>), typeof(DataEntityRepository<,>));
            services.AddTransient(typeof(IESRepository<,>), typeof(ESRepository<,>));
            services.AddTransient<IOrderRepository, OrderRepository>();
        }

        private static void AddServiceBus(this IServiceCollection services, IConfiguration configuration, List<Type> consumerTypes = default) {
            var options = GetServiceBusOptions(configuration);

            services.AddServiceBusMediator(consumerTypes);
            services.AddScoped<IServiceBusMediator, MassTransitMediator>();

            if (options.Enabled) {
                services.AddScoped<IServiceBus, MassTransitServiceBus>();

                switch (options.Provider.ToLower()) {
                    case "azure": services.AddAzureServiceBus(options.HostUrl, options.InputQueueName); break;
                    default:
                        throw new NotSupportedException($"The service bus provider '{options.Provider}' is not supported.");
                }
            }
            else {
                services.AddScoped<IServiceBus, MassTransitMediator>(); //registering the mediator as the service bus if no external service bus is used
            }
        }

        private static void AddServiceBusMediator(this IServiceCollection services, List<Type> consumerTypes = default) {
            services.AddMediator(m =>
            {
                if (consumerTypes != null && consumerTypes.Any()) {
                    consumerTypes.ForEach(t => m.AddConsumer(t));
                }
                else {
                    m.AddConsumers(typeof(CreateUserCommandHandler).Assembly);
                    m.AddConsumers(typeof(GetAllUsersQueryHandler).Assembly);
                }
            });
        }

        private static void AddAzureServiceBus(this IServiceCollection services, string hostUrl) {
            Guard.Against.NullOrEmpty(hostUrl, nameof(hostUrl));

            services.AddMassTransit(cfg =>
            {
                foreach (var eventType in GetAllMessageTypesFromAssemblies(typeof(CreateUserCommandHandler).Assembly, typeof(GetAllUsersQueryHandler).Assembly)) {
                    var consumerType = typeof(BusMessageConsumer<>).MakeGenericType(eventType);
                    cfg.AddConsumer(consumerType);
                }

                cfg.SetKebabCaseEndpointNameFormatter();
                cfg.UsingAzureServiceBus((context, config) =>
                {
                    config.UseServiceBusMessageScheduler();
                    config.AutoStart = true;
                    config.Host(hostUrl);

                    foreach (var eventType in GetAllMessageTypesFromAssemblies(typeof(CreateUserCommandHandler).Assembly, typeof(GetAllUsersQueryHandler).Assembly)) {
                        var consumerType = typeof(BusMessageConsumer<>).MakeGenericType(eventType);

                        config.ReceiveEndpoint(eventType.Name.ToLower(), e =>
                        {
                            e.ConfigureConsumer(context, consumerType);
                        });
                    }
                });
            });
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

            if (options.StorageProvider == "azure") {
                services.AddTransient<IResourceStore, AzureStorageResourceStore>();
            }
            else if (options.StorageProvider == "aws") {
                services.AddTransient<IResourceStore, AWSS3ResourceStore>();
            }

            services.AddTransient<IResourcesService, ResourcesService>();
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

        private static ServiceBusOptions GetServiceBusOptions(IConfiguration configuration) {
            ServiceBusOptions serviceBusOptions = new ServiceBusOptions();
            configuration.GetSection("ServiceBus").Bind(serviceBusOptions);

            return serviceBusOptions;
        }

        private static IEnumerable<Type> GetAllMessageTypesFromAssemblies(params Assembly[] assemblies) {
            return assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseMessage)));
        }
    }
}