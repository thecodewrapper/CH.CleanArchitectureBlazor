using CH.CleanArchitecture.Core.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CH.CleanArchitecture.Presentation.Framework.Components
{
    public partial class CRUDButton : ComponentBase
    {
        [Inject]
        public ILocalizationService LocalizationService { get; set; }

        [Parameter]
        public CRUDButtonType Type { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> OnClick { get; set; }

        [Parameter]
        public string Text { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public string Style { get; set; }
    }
}
