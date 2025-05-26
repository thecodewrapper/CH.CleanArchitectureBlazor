namespace CH.CleanArchitecture.Infrastructure.Options
{
    public class SMSSenderOptions
    {
        public bool UseTwilio { get; set; }
        public TwilioOptions Twilio { get; set; }
    }

    public class TwilioOptions
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }

        /// <summary>
        /// Used for Twilio's Verify API
        /// </summary>
        public string VerifySid { get; set; }
    }
}
