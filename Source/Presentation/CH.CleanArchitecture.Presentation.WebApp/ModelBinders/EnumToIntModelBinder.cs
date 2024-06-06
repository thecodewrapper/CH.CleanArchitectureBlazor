using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CH.CleanArchitecture.Presentation.WebApp.ModelBinders
{
    public class EnumToIntModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext) {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            var value = valueProviderResult.Values;

            int sum = 0;

            foreach (var item in value) {
                sum += int.Parse(item);
            }

            bindingContext.Result = ModelBindingResult.Success(sum);

            return Task.CompletedTask;
        }
    }
}
