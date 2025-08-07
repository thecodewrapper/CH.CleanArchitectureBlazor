using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.Framework.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.Developer
{
    public partial class EmailTester : BaseComponent
    {
        [Inject] private IEmailService _emailService { get; set; }

        private string _email;
        private string _status;
        private Severity _severity = Severity.Info;

        private async Task SendEmailAsync() {
            _status = "Sending...";
            _severity = Severity.Info;

            try {
                var result = await _emailService.SendEmailAsync(_email, "TestMail", "This is a test mail");

                if (result.IsSuccessful) {
                    _status = $"✅ Email sent successfully!";
                    _severity = Severity.Success;
                }
                else {
                    _status = $"❌ Failed to send email: {result.MessageWithErrors}";
                    _severity = Severity.Error;
                }
            }
            catch (Exception ex) {
                _status = $"❌ Exception: {ex.Message}";
                _severity = Severity.Error;
            }
        }

        private bool ValidateEmail(string value) {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            try {
                var addr = new System.Net.Mail.MailAddress(value);
                return true;
            }
            catch {
                return false;
            }
        }
    }
}
