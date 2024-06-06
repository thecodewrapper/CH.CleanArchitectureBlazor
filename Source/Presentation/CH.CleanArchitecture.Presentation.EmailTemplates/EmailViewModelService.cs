using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using CH.CleanArchitecture.Presentation.EmailTemplates.Models;

namespace CH.CleanArchitecture.Presentation.EmailTemplates
{
    internal class EmailViewModelService : IEmailViewModelService
    {
        public EmailViewModel<T> ConstructEmailViewModel<T>(T payload) {
            EmailViewModelBuilder<T> modelBuilder = new EmailViewModelBuilder<T>();

            return modelBuilder
                .WithPayload(payload)
                .Build();
        }
    }
}
