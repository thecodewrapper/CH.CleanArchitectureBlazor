using System;
using System.Collections.Generic;
using System.Text;
using CH.CleanArchitecture.Presentation.EmailTemplates.Models;

namespace CH.CleanArchitecture.Presentation.EmailTemplates
{
    public class EmailViewModelBuilder<T>
    {
        private EmailViewModel<T> _emailViewModel = new EmailViewModel<T>();

        public EmailViewModelBuilder() {
            Reset();
        }

        public void Reset() {
            _emailViewModel = new EmailViewModel<T>();
        }

        public EmailViewModel<T> Build() {
            return _emailViewModel;
        }

        public EmailViewModelBuilder<T> WithPayload(T payload) {
            _emailViewModel.Payload = payload;
            return this;
        }

        public EmailViewModelBuilder<T> WithCallToAction(string text, string url) {
            return WithCallToAction(new CTAViewModel() { Text = text, URL = url });
        }

        public EmailViewModelBuilder<T> WithCallToAction(CTAViewModel ctaVM) {
            _emailViewModel.CallToAction = ctaVM;
            return this;
        }
    }
}
