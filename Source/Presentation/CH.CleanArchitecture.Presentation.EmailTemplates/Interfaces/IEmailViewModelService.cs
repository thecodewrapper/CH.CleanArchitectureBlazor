using Microsoft.AspNetCore.Http;
using CH.CleanArchitecture.Presentation.EmailTemplates.Models;

namespace CH.CleanArchitecture.Presentation.EmailTemplates
{
    public interface IEmailViewModelService
    {
        EmailViewModel<T> ConstructEmailViewModel<T>(T payload);
    }
}
