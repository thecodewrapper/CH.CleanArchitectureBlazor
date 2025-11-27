using System;
using System.Linq;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Core.Application
{
    public abstract class BaseQueryHandler<TRequest, TResponse> : BaseMessageHandler<TRequest, TResponse>
        where TRequest : BaseQuery<TResponse>
        where TResponse : class, IResult
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger _logger;

        public BaseQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
            _authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
            _logger = serviceProvider.GetRequiredService<ILogger<TRequest>>();
        }

        public override async Task Consume(ConsumeContext<TRequest> context) {
            IdentityContext.Initialize(context.Message.IdentityContext?.Claims);
            var requirements = context.Message.Requirements;

            // Checking authorization requirements
            if (requirements.Any()) {
                var user = IdentityContext.User;
                var authorizationResult = await _authorizationService.AuthorizeAsync(user, null, requirements);

                if (!authorizationResult.Succeeded) {
                    _logger.LogError($"Authorization failed for {context.Message.GetType()}"); //push properties for failure reasons here
                    await context.RespondAsync(CreateFailedResponse("Authorization Failed"));
                    return;
                }
            }

            _logger.LogDebug($"Handling {context.Message.GetType()}");
            var messageResult = await HandleAsync(context.Message);
            _logger.LogDebug($"Handled {context.Message.GetType()}");
            await context.RespondAsync(messageResult);
        }

        protected TResponse CreateFailedResponse(string message, params IResultError[] errors) {
            var result = Activator.CreateInstance<TResponse>();
            if (result is Result r) {
                r.IsSuccessful = false;
                r.Message = message;

                foreach (var error in errors) {
                    r.AddError(error.Error, error.Code);
                }
            }
            else {
                throw new InvalidOperationException("TResponse must inherit from Result.");
            }

            return result;
        }
    }
}
