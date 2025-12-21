using System.Security.Claims;

namespace CH.CleanArchitecture.Core.Application
{
    public class AuthenticationStateService : IAuthenticationStateService
    {
        private readonly IIdentityContext _identityContext;
        private ClaimsPrincipal? _currentUser;
        private SemaphoreSlim _consistencySemaphore = new SemaphoreSlim(1, 1);

        public event CurrentUserChangedHandler CurrentUserChanged;

        public AuthenticationStateService(IIdentityContext identityContext) {
            _identityContext = identityContext;
        }

        public async Task<ClaimsPrincipal?> GetCurrentUserAsync() {
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
                if (_currentUser?.Identity == null || !_currentUser.Identity.IsAuthenticated) {
                    return;
                }

                _identityContext.Initialize(_currentUser.Claims.Select(c => new ClaimData(c.Type, c.Value)).ToList()); //update identity provider state

            }
            finally {
                _consistencySemaphore.Release();
                if (CurrentUserChanged is not null) {
                    await CurrentUserChanged.Invoke();
                } 
            }
        }
    }
}
