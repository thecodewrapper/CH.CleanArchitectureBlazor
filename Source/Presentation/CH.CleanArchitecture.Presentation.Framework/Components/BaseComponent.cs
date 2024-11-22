using AutoMapper;
using CH.Messaging.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.Framework.Interfaces;
using CH.CleanArchitecture.Presentation.Framework.Services;

namespace CH.CleanArchitecture.Presentation.Framework.Components
{
    public abstract class BaseComponent : ComponentBase, IDisposable
    {
        protected bool _disposedValue;
        #region Dependencies

        [Inject]
        public IServiceBus ServiceBus { get; set; }

        [Inject]
        public IMapper Mapper { get; set; }

        [Inject]
        public IToastService ToastService { get; set; }

        [Inject]
        public IModalService ModalService { get; set; }

        [Inject]
        public LoaderService Loader { get; set; }

        [Inject]
        public ILocalizationService LocalizationService { get; set; }

        [Inject]
        public IIdentityContext IdentityContext { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ILogger<BaseComponent> Logger { get; set; }

        #endregion

        #region Public Properties

        public bool ShouldRenderFlag { get; set; } = true;

        #endregion

        #region Public Properties

        protected override bool ShouldRender() {
            return ShouldRenderFlag;
        }

        #endregion

        #region Public Methods

        public Task<ClaimsPrincipal> GetCurrentUserAsync() {
            return Task.FromResult(IdentityContext.User);
        }

        public async Task StateHasChangedAsync() {
            await InvokeAsync(() => StateHasChanged());
        }

        #endregion

        #region Protected Methods
        protected virtual void LogExceptionError(Exception ex, string area) {
            Guid exceptionId = Guid.NewGuid();
            Logger.LogError($"Exception occured on {area} ({GetType().FullName}) - ExceptionId: {exceptionId}: {ex}");
            ModalService.ShowInfoModal("An error occured", $"Incident code: '{exceptionId}'. Please contact your administrator and provide the incident code.");
        }

        protected async Task<TResponse> SendRequestAsync<TResponse>(IRequest<TResponse> request, bool showLoader = true, CancellationToken cancellationToken = default) where TResponse : class {
            if (showLoader)
                Loader.Show();

            var result = await ServiceBus.SendAsync(request, cancellationToken);

            if (showLoader)
                Loader.Hide();

            return result;
        }

        protected async Task<TResponse> SendRequestAsync<TResponse>(IRequest<TResponse> request,
            string toastErrorMessage,
            string toastSuccessMessage = default,
            bool showLoader = true,
            CancellationToken cancellationToken = default) where TResponse : class, IResult {

            var result = await SendRequestAsync(request, showLoader, cancellationToken);

            if (result.IsSuccessful) {
                if (!string.IsNullOrEmpty(toastSuccessMessage)) {
                    ToastService.ShowSuccess(toastSuccessMessage);
                }
            }
            else {
                Logger.LogError($"Error: {toastErrorMessage}");
                ToastService.ShowError(toastErrorMessage);
            }

            return result;
        }

        protected virtual void DisposeComponent() {
            Mapper = null;
            ServiceBus = null;
            ToastService = null;
            ModalService = null;
            Loader = null;
            LocalizationService = null;
            NavigationManager = null;
            Logger = null;
        }

        private void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    DisposeComponent();
                }
                _disposedValue = true;
            }
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
