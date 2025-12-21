namespace CH.CleanArchitecture.Core.Application
{
    public interface IAccessTokenService
    {
        Task<TokenResult> IssueTokenAsync(IIdentityContext identityContext, CancellationToken ct);
    }
}
