using System.Security.Claims;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Presentation.WebApp.Services
{
    public class AuthenticationStateService : IAuthenticationStateService
    {
        private readonly ILogger<AuthenticationStateService> _logger;
        private readonly IIdentityContext _identityContext;
        private ClaimsPrincipal? _currentUser;
        private SemaphoreSlim _consistencySemaphore = new SemaphoreSlim(1, 1);

        public event CurrentUserChangedHandler CurrentUserChanged;

        public AuthenticationStateService(ILogger<AuthenticationStateService> logger, IIdentityContext identityContext) {
            _logger = logger;
            _identityContext = identityContext;
        }

        public async Task<ClaimsPrincipal> GetCurrentUserAsync() {
            await _consistencySemaphore.WaitAsync();
            try {
                return _currentUser;
            }
            finally {
                _consistencySemaphore.Release();
            }
        }

        /// <summary>
        /// Sets the current user with the given <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task SetCurrentUserAsync(ClaimsPrincipal user) {
            await _consistencySemaphore.WaitAsync();
            try {
                _currentUser = user;
                _logger.LogInformation("Current user in AuthenticationStateService is set to: {@user}", _currentUser.FindFullName());

                if (_currentUser?.Identity == null || !_currentUser.Identity.IsAuthenticated) {
                    return;
                }

                _identityContext.Initialize(_currentUser); //update identity provider state

            }
            finally {
                _consistencySemaphore.Release();
                await CurrentUserChanged.Invoke();
            }
        }
    }
}
