namespace CH.CleanArchitecture.Infrastructure.Options
{
    public sealed class IdentityProviderOptions
    {
        /// <summary>
        /// Token issuer (iss)
        /// </summary>
        public string Issuer { get; set; } = default!;

        /// <summary>
        /// Audience this IdP is allowed to issue tokens for
        /// </summary>
        public string Audience { get; set; } = default!;

        /// <summary>
        /// Access token lifetime in minutes
        /// </summary>
        public int AccessTokenLifetimeMinutes { get; set; } = 10;
    }
}
