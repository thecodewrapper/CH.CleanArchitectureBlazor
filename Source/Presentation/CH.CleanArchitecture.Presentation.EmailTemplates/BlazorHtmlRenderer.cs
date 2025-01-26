using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;

namespace CH.CleanArchitecture.Presentation.EmailTemplates
{
    internal class BlazorHtmlRenderer : IComponentHtmlRenderer
    {
        private readonly HtmlRenderer _htmlRenderer;
        public BlazorHtmlRenderer(HtmlRenderer htmlRenderer) {
            _htmlRenderer = htmlRenderer;
        }

        // Renders a component T which doesn't require any parameters
        public Task<string> RenderComponentAsync<T>() where T : IComponent
            => RenderComponent<T>(ParameterView.Empty);

        // Renders a component T using the provided dictionary of parameters
        public Task<string> RenderComponentAsync<T>(Dictionary<string, object?> dictionary) where T : IComponent
            => RenderComponent<T>(ParameterView.FromDictionary(dictionary));

        private Task<string> RenderComponent<T>(ParameterView parameters) where T : IComponent {
            // Use the default dispatcher to invoke actions in the context of the 
            // static HTML renderer and return as a string
            return _htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                HtmlRootComponent output = await _htmlRenderer.RenderComponentAsync<T>(parameters);
                return output.ToHtmlString();
            });
        }
    }
}
