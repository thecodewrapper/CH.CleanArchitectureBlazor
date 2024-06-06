using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.Resources;

namespace CH.CleanArchitecture.Presentation.WebApp.Helpers
{
    public static class LocalizationHelper
    {
        public static string GetLocalizedAddNewModalTitle(ILocalizationService localizationService, string resourceKey) {
            return localizationService.GetLocalizedString(ResourceKeys.Titles_AddNewModal).ToString()
                .Replace("{0}", localizationService.GetLocalizedString(resourceKey).ToString().ToLower());
        }

        public static string GetYesNo(this ILocalizationService localizationService, bool value) {
            return value ? localizationService.GetLocalizedString(ResourceKeys.Common_Yes) : localizationService.GetLocalizedString(ResourceKeys.Common_No);
        }
    }
}
