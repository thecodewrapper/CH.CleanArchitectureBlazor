using System;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IResourceStoreSecurityService
    {
        public Task<string> CreateReadOnlySecurityTokenAsync(string resourceName, string resourceType, DateTimeOffset expirationDate);
    }
}
