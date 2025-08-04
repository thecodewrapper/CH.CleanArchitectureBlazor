using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.Admin
{
    public partial class ResourceForm : FormComponent
    {
        [Inject]
        public IResourcesService _resourcesService { get; set; }

        private List<IBrowserFile> _files = new List<IBrowserFile>();
        private ResourceFormModel _formModel = new ResourceFormModel();
        private readonly int _maxAllowedFiles = 1;
        private readonly int _maxFileSize = 10 * 1024 * 1024;

        protected override Task OnInitializedAsync() {
            return base.OnInitializedAsync();
        }

        protected override async Task<Result> HandleAsync() {
            if (_files.Count == 0)
            {
                return new Result { IsSuccessful = false, Message = "No file selected." };
            }

            var file = _files[0];
            await using var stream = file.OpenReadStream(_maxFileSize); // Enforce max file size
            var result = await _resourcesService.AddResourceAsync(
                stream, _formModel.FolderName,
                _formModel.Name,
                _formModel.Type,
                _formModel.IsPublic
            );
            return result;
        }

        private void SelectFiles(IBrowserFile file) {
            _files.Add(file);
        }
    }
}
