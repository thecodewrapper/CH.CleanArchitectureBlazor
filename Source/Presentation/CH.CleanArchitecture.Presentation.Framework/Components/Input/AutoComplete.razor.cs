using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Presentation.Framework.Components.Input
{
    public partial class AutoComplete<T> : InputBase<T>
    {
        [Parameter]
        public Expression<Func<T>> For { get; set; }
        /// <summary>
        /// If true, the input will update the Value immediately on typing. If false, the Value is updated only on Enter.
        /// </summary>
        [Parameter]
        public bool Immediate { get; set; } = false;

        /// <summary>
        /// Interval to be awaited in milliseconds before changing the Text value
        /// </summary>
        [Parameter]
        public int DebounceInterval { get; set; } = 300;

        /// <summary>
        /// Reset value if user deletes the text
        /// </summary>
        [Parameter]
        public bool ResetValueOnEmptyText { get; set; } = false;

        /// <summary>
        /// Show clear button.
        /// </summary>
        [Parameter]
        public bool Clearable { get; set; } = false;

        /// <summary>
        /// If false, clicking on the Autocomplete after selecting an option will query the Search method again with an empty string. This makes it easier to view and select other options without resetting the Value. T must either be a record or override GetHashCode and Equals.
        /// </summary>
        [Parameter]
        public bool Strict { get; set; } = true;

        /// <summary>
        /// On drop-down close override Text with selected Value. This makes it clear to the user which list value is currently selected and disallows incomplete values in Text.
        /// </summary>
        [Parameter]
        public bool CoerceText { get; set; } = true;

        /// <summary>
        /// If user input is not found by the search func and CoerceValue is set to true the user input will be applied to the Value which allows to validate it and display an error message.
        /// </summary>
        [Parameter]
        public bool CoerceValue { get; set; } = false;

        [Parameter]
        /// <summary>
        /// Indicates whether to show the progress indicator.
        /// </summary>
        public bool ShowProgressIndicator { get; set; } = true;

        /// <summary>
        /// The color of the progress indicator
        /// </summary>
        public Color ProgressIndicatorColor { get; set; } = Color.Primary;

        /// <summary>
        /// The SearchFunc returns a list of items matching the typed text
        /// </summary>
        [Parameter]
        public Func<string, CancellationToken, Task<IEnumerable<T>>> SearchFunc { get; set; }

        [Parameter]
        public Func<string, CancellationToken, Task<IEnumerable<T>>> SearchFuncWithCancel { get; set; }
        /// <summary>
        /// Defines how values are displayed in the drop-down list
        /// </summary>
        [Parameter]
        public Func<T, string> ToStringFunc { get; set; } = null;

        /// <summary>
        /// Fired when the Text property changes.
        /// </summary>
        [Parameter]
        public EventCallback<string> TextChanged { get; set; }

        /// <summary>
        /// Button click event for clear button. Called after text and value has been cleared.
        /// </summary>
        [Parameter]
        public EventCallback<MouseEventArgs> OnClearButtonClick { get; set; }

        /// <summary>
        /// Fired when the element loses focus.
        /// </summary>
        [Parameter]
        public EventCallback<FocusEventArgs> OnBlur { get; set; }

        /// <summary>
        /// An event triggered when the state of IsOpen has changed
        /// </summary>
        [Parameter]
        public EventCallback<bool> IsOpenChanged { get; set; }

        [Parameter]
        public Margin Margin { get; set; } = Margin.None;

        [Parameter]
        public int? MaxItems { get; set; } = default;

        private MudAutocomplete<T> _autoComplete;

        private async Task OnClearButtonClicked(MouseEventArgs args) {
            if (OnClearButtonClick.HasDelegate) {
                await OnClearButtonClick.InvokeAsync(args);
            }
        }

        private async Task OnBlurChanged(FocusEventArgs args) {
            if (OnBlur.HasDelegate) {
                await OnBlur.InvokeAsync(args);
            }
        }

        private async Task OpenChanged(bool isOpen) {
            if (IsOpenChanged.HasDelegate) {
                await IsOpenChanged.InvokeAsync(isOpen);
            }
        }

        public Task ForceUpdate() {
            return _autoComplete.ForceUpdate();
        }

        public Task SelectOption(T value) {
            return _autoComplete.SelectOptionAsync(value);
        }
    }
}
