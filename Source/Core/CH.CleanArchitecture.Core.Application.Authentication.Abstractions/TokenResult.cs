namespace CH.CleanArchitecture.Core.Application
{
    public sealed record TokenResult(string AccessToken, int ExpiresInSeconds);
}
