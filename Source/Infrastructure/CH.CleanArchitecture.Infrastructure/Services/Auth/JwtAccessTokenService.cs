using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal sealed class JwtAccessTokenService : IAccessTokenService
    {
        private readonly ILogger<JwtAccessTokenService> _logger;
        private readonly ISigningKeyProvider _keys;
        private readonly IdentityProviderOptions _options;

        public JwtAccessTokenService(ILogger<JwtAccessTokenService> logger, ISigningKeyProvider keys, IOptions<IdentityProviderOptions> options) {
            _logger = logger;
            _keys = keys;
            _options = options.Value;
        }

        public async Task<TokenResult> IssueTokenAsync(IIdentityContext identityContext, CancellationToken ct) {
            if (identityContext == null) throw new ArgumentNullException(nameof(identityContext));
            if (string.IsNullOrWhiteSpace(identityContext.UserId))
                throw new InvalidOperationException("IdentityContext.UserId is empty.");

            var lifetimeMinutes = _options.AccessTokenLifetimeMinutes;

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(lifetimeMinutes);

            // Standard + app claims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, identityContext.UserId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(JwtRegisteredClaimNames.Iat, Epoch(now).ToString(), ClaimValueTypes.Integer64),

                new(ClaimTypes.Name, identityContext.Name ?? identityContext.Username ?? identityContext.UserId),
            };

            claims.Add(new Claim("client_id", identityContext.ClientId ?? ""));

            // Roles (enables [Authorize(Roles="Admin")] on the API)
            if (identityContext.Roles != null) {
                foreach (var role in identityContext.Roles.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct())
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Optional: forward selected claims from your identity context
            // Avoid blindly forwarding everything if some claims are big/sensitive.
            if (identityContext.Claims != null) {
                foreach (var c in identityContext.Claims) {
                    if (string.IsNullOrWhiteSpace(c?.Type) || c.Value == null)
                        continue;

                    claims.Add(new Claim(c.Type, c.Value));
                }
            }

            var creds = await _keys.GetSigningCredentialsAsync(ct);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            token.Header["kid"] = _keys.GetKeyId();

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Issued API access token for UserId={UserId}, ClientId={ClientId}, Exp={Exp:o}",
                identityContext.UserId, identityContext.ClientId, expires);

            return new TokenResult(jwt, (int)(expires - now).TotalSeconds);
        }

        private static long Epoch(DateTime utc)
            => (long)Math.Floor((utc.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds);
    }
}
