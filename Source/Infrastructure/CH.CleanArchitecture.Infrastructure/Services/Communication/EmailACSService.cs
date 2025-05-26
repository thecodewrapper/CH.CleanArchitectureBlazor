using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.Constants;
using CH.CleanArchitecture.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Provides functionality for sending emails using Azure Communication Services (ACS).
    /// </summary>
    /// <remarks>This service supports sending emails to one or multiple recipients, with options to specify
    /// the sender's email address. It integrates with Azure Communication Services for email delivery and logs any
    /// errors encountered during the process.</remarks>
    internal class EmailACSService : IEmailService
    {
        private readonly IApplicationConfigurationService _applicationConfigurationService;
        private readonly string _senderEmail;
        private readonly ILogger<EmailACSService> _logger;
        private readonly AzureCommunicationServicesOptions _options;

        public EmailACSService(ILogger<EmailACSService> logger, IApplicationConfigurationService applicationConfigurationService, IOptions<EmailSenderOptions> options) {
            _logger = logger;
            _applicationConfigurationService = applicationConfigurationService;
            _senderEmail = _applicationConfigurationService.GetValue(AppConfigKeys.EMAIL.FROM_ADDRESS).Unwrap().Trim();
            _options = options.Value.Azure;
        }

        public async Task<Result> SendEmailAsync(string from, string to, string subject, string message) {
            return await SendInternalAsync(from, new List<string> { to }, subject, message);
        }

        public async Task<Result> SendEmailAsync(string to, string subject, string message) {
            return await SendEmailAsync(_senderEmail, to, subject, message);
        }

        public async Task<Result> SendEmailAsync(string from, List<string> tos, string subject, string message) {
            return await SendInternalAsync(from, tos, subject, message);
        }

        public async Task<Result> SendEmailAsync(List<string> tos, string subject, string message) {
            return await SendEmailAsync(_senderEmail, tos, subject, message);
        }

        private async Task<Result> SendInternalAsync(string from, List<string> tos, string subject, string message) {
            EmailClient emailClient = new EmailClient(_options.ConnectionString);
            EmailMessage emailMessage = ConstructMessage(from, tos, subject, message);

            try {
                var emailSendOperation = await emailClient.SendAsync(WaitUntil.Completed, emailMessage);

                if (emailSendOperation.HasCompleted) {
                    return new Result().Succeed();
                }

                _logger.LogError("Email sending did not complete. From: {From}, To: {To}, Subject: {Subject}", from, string.Join(",", tos), subject);
                return new Result().Fail().WithError("Email send operation did not complete.");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error sending email. From: {From}, To: {To}, Subject: {Subject}", from, string.Join(",", tos), subject);
                return new Result().Fail().WithError("Exception occurred while sending email.");
            }
        }

        private EmailMessage ConstructMessage(string from, List<string> tos, string subject, string body) {
            return new EmailMessage(
                senderAddress: from,
                content: new EmailContent(subject)
                {
                    Html = body
                },
                recipients: new EmailRecipients(tos.ConvertAll(email => new EmailAddress(email)))
            );
        }
    }
}