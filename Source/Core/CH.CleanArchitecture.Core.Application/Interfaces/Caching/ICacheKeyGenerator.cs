namespace CH.CleanArchitecture.Core.Application
{
    public interface ICacheKeyGenerator
    {
        string GenerateKey(string baseKey, params string[] parameters);
        string GenerateKey<T>(T type, params string[] parameters);
    }
}
