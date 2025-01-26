using Microsoft.AspNetCore.Components;

namespace CH.CleanArchitecture.Presentation.Framework.Components
{
    public partial class PageHeader : ComponentBase
    {
        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public string Subtitle { get; set; }

        [Parameter]
        public string Subtitle2 { get; set; }

        [Parameter]
        public string Subtitle3 { get; set; }

        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public string Icon { get; set; }

        [Parameter]
        public EventCallback OnSubtitleClick { get; set; }

        [Parameter]
        public EventCallback OnSubtitle2Click { get; set; }

        [Parameter]
        public EventCallback OnSubtitle3Click { get; set; }
    }
}
