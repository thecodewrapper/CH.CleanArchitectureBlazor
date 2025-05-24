using System;
using System.Linq;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Core.Application
{
    public abstract class BaseCommandHandler<TRequest, TResponse> : BaseMessageHandler<TRequest, TResponse>
        where TRequest : BaseCommand<TResponse>
        where TResponse : class, IResult
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger _logger;
        private readonly IValidator<TRequest> _validator;

        public BaseCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
            _authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
            _logger = serviceProvider.GetRequiredService<ILogger<TRequest>>();
            _validator = serviceProvider.GetService<IValidator<TRequest>>();
        }

        public override async Task Consume(ConsumeContext<TRequest> context) {
            IdentityContext.Initialize(context.Message.IdentityContext.Claims);
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

            // Perform validation, if any
            if (_validator != null) {
                _logger.LogDebug($"Validating command {context.Message.GetType()}");
                FluentValidation.Results.ValidationResult validationResult = await _validator.ValidateAsync(context.Message);

                if (!validationResult.IsValid) {
                    _logger.LogError($"Validation failed for {context.Message.GetType()}. Validation Errors: {string.Join(",", validationResult.Errors.Select(e => e.ErrorMessage))}");

                    var failedResponse = CreateFailedResponse("Validation Failed", [.. validationResult.Errors.Select(e => new ResultError(e.ErrorMessage, e.ErrorCode))]);
                    await context.RespondAsync(failedResponse);
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
