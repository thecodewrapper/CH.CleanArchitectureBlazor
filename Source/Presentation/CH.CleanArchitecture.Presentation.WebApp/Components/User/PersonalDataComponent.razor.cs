using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Text;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Common.Extensions;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.Framework.Components;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.User
{
    public partial class PersonalDataComponent : BaseComponent
    {
        [Inject] private IApplicationUserService _applicationUserService { get; set; }
        [Inject] private IJSRuntime _jsRuntime { get; set; }

        public async Task DownloadPersonalData() {
            var user = await GetCurrentUserAsync();
            Logger.LogInformation($"User '{user.FindFullName()} ({user.FindId()})' asked for their personal data.");

            // Only include personal data for download
            var personalDataResult = await _applicationUserService.GetUserPersonalDataAsync(user.FindId());

            if (personalDataResult.IsSuccessful) {
                var personalData = personalDataResult.Unwrap();
                var stream = GetFileStream(JsonConvert.SerializeObject(personalData));
                await DownloadFileFromStream(stream);
            }
        }

        private Stream GetFileStream(string fileContents) {
            var binaryData = Encoding.UTF8.GetBytes(fileContents);
            var fileStream = new MemoryStream(binaryData);

            return fileStream;
        }

        private async Task DownloadFileFromStream(Stream stream) {
            var fileName = "personalData.json";

            using var streamRef = new DotNetStreamReference(stream: stream);

            await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }
    }
}
