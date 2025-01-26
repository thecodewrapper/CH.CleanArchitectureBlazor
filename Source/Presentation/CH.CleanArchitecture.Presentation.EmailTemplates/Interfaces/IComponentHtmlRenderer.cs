using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace CH.CleanArchitecture.Presentation.EmailTemplates
{
    public interface IComponentHtmlRenderer
    {
        Task<string> RenderComponentAsync<T>() where T : IComponent;
        Task<string> RenderComponentAsync<T>(Dictionary<string, object?> dictionary) where T : IComponent;
    }
}
