using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Presentation.EmailTemplates
{
    public interface IComponentHtmlRenderer
    {
        Task<string> RenderComponentAsync<T>() where T : IComponent;
        Task<string> RenderComponentAsync<T>(Dictionary<string, object?> dictionary) where T : IComponent;
    }
}
