using System.Text.Encodings.Web;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Domain.Entities.UserAggregate;

namespace CH.CleanArchitecture.Presentation.WebApp.Services
{
    public class UserAccountService
    {
        private readonly ILogger<UserAccountService> _logger;
        private readonly IUrlTokenService _urlTokenService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly LinkGenerator _linkGenerator;
        private readonly IUserNotificationService _userNotificationService;

        public UserAccountService(ILogger<UserAccountService> logger, IUrlTokenService urlTokenService, IApplicationUserService applicationUserService, LinkGenerator linkGenerator, IUserNotificationService userNotificationService) {
            _logger = logger;
            _urlTokenService = urlTokenService;
            _applicationUserService = applicationUserService;
            _linkGenerator = linkGenerator;
            _userNotificationService = userNotificationService;
        }

        public async Task SendConfirmationEmailAsync(string email, string baseUri) {
            var user = (await _applicationUserService.GetUserByEmailAsync(email)).Unwrap();
            if (user == null) {
                _logger.LogWarning($"User is null. Aborting sending confirmation email");
                return;
            }

            var tokenResult = await _applicationUserService.GenerateEmailConfirmationTokenAsync(email);
            if (tokenResult.IsFailed) {
                _logger.LogError($"Email confirmation token generation failed for email {email}. Aborting sending confirmation email...");
                return;
            }
            await SendConfirmationEmailInternal(tokenResult.Unwrap(), user, baseUri);
        }

        public async Task SendResetPasswordEmailAsync(string email, string baseUri) {
            var user = (await _applicationUserService.GetUserByEmailAsync(email)).Unwrap();
            if (user == null) {
                _logger.LogWarning($"User is null. Aborting sending password reset email");
                return;
            }

            var result = await _applicationUserService.GeneratePasswordResetTokenAsync(email);
            if (result.IsFailed) {
                _logger.LogError($"Password reset token generation failed for email {email}. Aborting sending password reset email...");
                return;
            }

            string resetToken = result.Unwrap();
            await SendResetPasswordEmailInternal(resetToken, user, baseUri);
        }

        public string DecodeEmailToken(string token) {
            return _urlTokenService.ReadSafeUrlToken<string>(token);
        }

        private async Task SendConfirmationEmailInternal(string confirmationToken, User user, string baseUri) {
            _logger.LogInformation($"Sending confirmation email to '{user.Email}' for user '{user.Id}'. Token: {confirmationToken}");
            string code = _urlTokenService.CreateSafeUrlToken(confirmationToken);
            string callbackRelativeUrl = _linkGenerator.GetPathByPage("/Account/ConfirmEmail", null, values: new { userId = user.Id, code });
            string callbackUrl = new Uri(new Uri(baseUri), callbackRelativeUrl).AbsoluteUri;

            await _userNotificationService.NotifyUserForAccountConfirmationAsync(user, callbackUrl);

            _logger.LogInformation($"Confirmation email sent to '{user.Email}'. Callback URL: {callbackUrl}");
        }

        private async Task SendResetPasswordEmailInternal(string resetToken, User user, string baseUri) {
            _logger.LogInformation($"Sending password reset email to '{user.Email}'. Token: {resetToken}");
            string code = _urlTokenService.CreateSafeUrlToken(resetToken);
            var callbackRelativeUrl = _linkGenerator.GetPathByPage("/Account/ResetPassword", null, values: new { code });
            string callbackUrl = new Uri(new Uri(baseUri), callbackRelativeUrl).AbsoluteUri;

            await _userNotificationService.NotifyUserForResetPasswordAsync(user, callbackUrl);

            _logger.LogInformation($"Password reset email sent to '{user.Email}'. Callback URL: {callbackUrl}");
        }
    }
}
