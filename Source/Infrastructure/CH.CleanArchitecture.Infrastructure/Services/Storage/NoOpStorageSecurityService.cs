using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// A no-operation implementation of the <see cref="IResourceStoreSecurityService"/> interface.
    /// </summary>
    internal class NoOpStorageSecurityService : IResourceStoreSecurityService
    {
        public Task<string> CreateReadOnlySecurityTokenAsync(string resourceName, string resourceType, DateTimeOffset expirationDate) {
            return Task.FromResult(string.Empty);
        }
    }
}
