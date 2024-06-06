using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace CH.CleanArchitecture.Presentation.WebApp.ModelBinders
{
    public class DecimalCommaModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext) {
            if (bindingContext == null) {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None) {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var valueAsString = valueProviderResult.FirstValue;

            // Error if dots are present
            if (valueAsString.Contains(".")) {
                bindingContext.ModelState.AddModelError(modelName, "Dots are not allowed. Please use a comma for decimal places.");
                return Task.CompletedTask;
            }

            // Error if more than one comma is present
            if (valueAsString.Count(f => f == ',') > 1) {
                bindingContext.ModelState.AddModelError(modelName, "Multiple commas are not allowed.");
                return Task.CompletedTask;
            }

            // Convert comma to dot for parsing, enforce culture
            var cultureInvariantValue = valueAsString.Replace(',', '.');

            if (decimal.TryParse(cultureInvariantValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)) {
                // Check for more than two decimal places
                if ((result * 100) % 1 != 0) // Multiplying by 100 and checking modulo 1 to see if there are more than two decimal digits
                {
                    bindingContext.ModelState.AddModelError(modelName, "No more than two decimal places are allowed.");
                    return Task.CompletedTask;
                }

                bindingContext.Result = ModelBindingResult.Success(result);
            }
            else {
                bindingContext.ModelState.AddModelError(modelName, "Invalid number format.");
            }

            return Task.CompletedTask;
        }
    }
}
