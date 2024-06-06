using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.Framework.Components.Input
{
    public abstract class InputBase<T> : BaseComponent
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Class { get; set; }

        [Parameter]
        public string Style { get; set; }

        [Parameter]
        public string Label { get; set; }

        [Parameter]
        public Variant Variant { get; set; } = Variant.Text;

        [Parameter]
        public InputType InputType { get; set; } = InputType.Text;

        [Parameter]
        public T Value
        {
            get => _value;
            set => _value = value;
        }

        [Parameter]
        public bool Required { get; set; } = false;

        [Parameter]
        public string RequiredError { get; set; } = "Required";

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public bool ReadOnly { get; set; }

        [Parameter]
        public object Validation { get; set; } = null;

        [Parameter]
        public bool Dense { get; set; }

        [Parameter]
        public EventCallback<T> ValueChanged { get; set; }

        private T _value;
    }
}
