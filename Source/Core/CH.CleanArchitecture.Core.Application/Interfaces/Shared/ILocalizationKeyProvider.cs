using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Core.Application
{
    /// <summary>
    /// Provides various localization keys for resolving to localized resources
    /// </summary>
    public interface ILocalizationKeyProvider
    {
        string GetRoleLocalizationKey(RoleEnum role);
    }
}
