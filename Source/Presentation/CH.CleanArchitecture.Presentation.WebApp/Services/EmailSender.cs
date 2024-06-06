using Microsoft.AspNetCore.Identity.UI.Services;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Presentation.WebApp.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly IEmailService _emailService;

        public EmailSender(ILogger<EmailSender> logger, IEmailService emailService) {
            _logger = logger;
            _emailService = emailService;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage) {
            var emailResult = await _emailService.SendEmailAsync(email, subject, htmlMessage);
            if (emailResult.IsSuccessful) {
                _logger.LogDebug($"Sent email to {email}. Contents: {htmlMessage}");
            }
        }
    }
}
