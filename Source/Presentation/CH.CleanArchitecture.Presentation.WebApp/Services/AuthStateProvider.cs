using System.Security.Claims;
using CH.CleanArchitecture.Core.Application;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CH.CleanArchitecture.Presentation.WebApp
{
    public class AuthStateProvider<TUser> : RevalidatingServerAuthenticationStateProvider where TUser : class
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAuthenticationStateService _authenticationStateService;
        private readonly ILogger<AuthStateProvider<TUser>> _logger;
        private readonly IdentityOptions _options;

        public AuthStateProvider(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory scopeFactory,
            IOptions<IdentityOptions> optionsAccessor,
            IAuthenticationStateService authenticationStateService,
            ILogger<AuthStateProvider<TUser>> logger)
            : base(loggerFactory) {
            _scopeFactory = scopeFactory;
            _authenticationStateService = authenticationStateService;
            _logger = logger;
            _options = optionsAccessor.Value;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
            AuthenticationState authenticationState = ConstructEmptyAuthenticationState();
            try {
                var currentUser = await _authenticationStateService.GetCurrentUserAsync();
                if (currentUser != null) {
                    _logger.LogDebug($"CurrentUser in {nameof(GetAuthenticationStateAsync)}: AuthenticationType: {currentUser.Identity?.AuthenticationType}, IsAuthenticated: {currentUser.Identity?.IsAuthenticated}");
                    authenticationState = new AuthenticationState(currentUser);
                }
            }
            catch (Exception ex) {
                _logger.LogError($"Unable to get authentication state from {nameof(AuthStateProvider<TUser>)}. Returning empty identity. Exception: {ex}");
            }

            return authenticationState;
        }

        protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(10);

        protected override async Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState, CancellationToken cancellationToken) {
            // Get the user manager from a new scope to ensure it fetches fresh data
            //var scope = _scopeFactory.CreateScope();
            //try {
            //    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TUser>>();
            //    return await ValidateSecurityStampAsync(userManager, authenticationState.User);
            //}
            //finally {
            //    if (scope is IAsyncDisposable asyncDisposable) {
            //        await asyncDisposable.DisposeAsync();
            //    }
            //    else {
            //        scope.Dispose();
            //    }
            //}
            return true;
        }

        private async Task<bool> ValidateSecurityStampAsync(UserManager<TUser> userManager, ClaimsPrincipal principal) {
            var user = await userManager.GetUserAsync(principal);
            if (user == null) {
                return false;
            }
            else if (!userManager.SupportsUserSecurityStamp) {
                return true;
            }
            else {
                var principalStamp = principal.FindFirstValue(_options.ClaimsIdentity.SecurityStampClaimType);
                var userStamp = await userManager.GetSecurityStampAsync(user);
                return principalStamp == userStamp;
            }
        }

        private async Task CurrentUserChanged() {
            var currentUser = await _authenticationStateService.GetCurrentUserAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(ConstructAuthenticationState(currentUser)));
        }

        private static AuthenticationState ConstructEmptyAuthenticationState() {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        private static AuthenticationState ConstructAuthenticationState(ClaimsPrincipal claimsPrincipal) {
            return new AuthenticationState(claimsPrincipal);
        }
    }
}