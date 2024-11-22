using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Resources;

namespace CH.CleanArchitecture.Presentation.Framework.Attributes
{
    public class LocalizedRequiredAttribute : RequiredAttribute
    {
        public LocalizedRequiredAttribute() {
            ErrorMessageResourceType = typeof(SharedResources);
            ErrorMessageResourceName = nameof(ResourceKeys.Validations_Required);
        }
    }
}
