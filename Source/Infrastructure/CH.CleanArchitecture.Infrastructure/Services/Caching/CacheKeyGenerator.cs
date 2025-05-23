using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.Constants;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    public class CacheKeyGenerator : ICacheKeyGenerator
    {
        private readonly IApplicationConfigurationService _appConfigService;
        private readonly string _keyPrefix;

        public CacheKeyGenerator(IApplicationConfigurationService appConfigService) {
            _appConfigService = appConfigService;
            _keyPrefix = _appConfigService.GetValue(AppConfigKeys.CACHE.KEY_PREFIX).Unwrap();
        }

        public string GenerateKey(string baseKey, params string[] parameters) {
            return $"{_keyPrefix}_{baseKey}_{string.Join('_', parameters)}";
        }

        public string GenerateKey<T>(T type, params string[] parameters) {
            return $"{_keyPrefix}_{typeof(T).Name}_{string.Join('_', parameters)}";
        }
    }
}
