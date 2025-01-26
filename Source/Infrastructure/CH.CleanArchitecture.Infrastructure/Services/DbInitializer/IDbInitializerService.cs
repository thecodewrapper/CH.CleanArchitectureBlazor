namespace CH.CleanArchitecture.Infrastructure.Services
{
    public interface IDbInitializerService
    {
        void Migrate();
        void Seed();
    }
}
