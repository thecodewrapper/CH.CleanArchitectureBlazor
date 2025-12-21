using Microsoft.IdentityModel.Tokens;

namespace CH.CleanArchitecture.Core.Application
{
    public interface ISigningKeyProvider
    {
        Task<SigningCredentials> GetSigningCredentialsAsync(CancellationToken ct);
        string GetKeyId();
        SecurityKey GetValidationKey();
    }
}
