using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CH.CleanArchitecture.Presentation.Framework.Components.Input;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.Framework.Components
{
    public partial class MultiSelectAutoComplete<TItem, TId>
    {
#nullable enable
        [Parameter]
        public IEnumerable<TItem>? Values { get; set; }
#nullable restore
        [Parameter]
        public EventCallback<IEnumerable<TItem>> ValuesChanged { get; set; }

        [Parameter]
        public string Label { get; set; }

        [Parameter, EditorRequired]
        public Func<string, Task<IEnumerable<TItem>>> SearchFunc { get; set; }

        [Parameter, EditorRequired]
        public Func<TItem, string> GetNameFunc { get; set; }

        [Parameter, EditorRequired]
        public Func<TItem, TId> GetIdFunc { get; set; }

        [Parameter]
        public Func<string, TItem> GetOtherFromStringFunc { get; set; }

        [Parameter]
        public bool AllowOtherValues { get; set; } = false;

        [Parameter]
        public Adornment Adornment { get; set; } = default;

        [Parameter]
        public string AdornmentIcon { get; set; } = default;

        private HashSet<TItem> _selectedValues = new HashSet<TItem>();
        private AutoComplete<TItem> _autoComplete;
        protected override void OnParametersSet() {
            base.OnParametersSet();

            if (Values == null) {
                _selectedValues.Clear();
            }
            else if (!Values.SequenceEqual(_selectedValues)) {
                _selectedValues = Values.ToHashSet();
                ValuesChanged.InvokeAsync(Values);
            }
        }

        private void RefreshBinding() {
            Values = _selectedValues.ToList();
            ValuesChanged.InvokeAsync(Values);
            StateHasChanged();
        }

        private void RemoveValue(MudChip<TItem> chip) {
            if (_selectedValues.RemoveWhere(x => GetNameFunc(x) == chip.Text) > 0)
                RefreshBinding();
        }

        private void AddValue(TItem newValue) {
            if (newValue != null) {
                if (_selectedValues.Add(newValue)) {
                    RefreshBinding();
                }
            }
        }

        private async Task<IEnumerable<TItem>> SearchAsync(string searchValue, CancellationToken cancellationToken) {
            var items = (await SearchFunc(searchValue)).ToList();

            if (AllowOtherValues) {
                if (!string.IsNullOrEmpty(searchValue) && !items.Any(m => string.Equals(GetNameFunc(m), searchValue, StringComparison.OrdinalIgnoreCase))) {
                    items.Add(GetOtherFromStringFunc(searchValue));
                    items.Reverse();
                }
            }

            return items;
        }

        /// <summary>
        /// Note that this is required to a) clear the control after you add
        /// an item to the list, and b) to trigger the addvalue method.
        /// If MudAutoComplete's bind-Value:after worked, we could get rid
        /// of this and just clear the value after it was added.
        /// </summary>
        private TItem Value
        {
            get => default;
            set { AddValue(value); }
        }
    }
}
