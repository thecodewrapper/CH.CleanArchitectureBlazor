using Microsoft.AspNetCore.Components.Forms;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Common.Enumerations;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Interfaces.Storage;
using CH.CleanArchitecture.Infrastructure.Constants;

namespace CH.CleanArchitecture.Presentation.WebApp.Services
{
    public class DocumentsService
    {
        private const int MAX_FILE_SIZE_PROFILEPICTURE = 2097152; //2MB
        private readonly ILogger<DocumentsService> _logger;
        private readonly IResourcesService _resourcesService;
        private readonly string _profilePicturesFolder;

        public DocumentsService(ILogger<DocumentsService> logger, IResourcesService resourcesService, IApplicationConfigurationService appConfigService) {
            _logger = logger;
            _resourcesService = resourcesService;
            _profilePicturesFolder = appConfigService.GetValue(AppConfigKeys.RESOURCES.RESOURCES_PROFILEPICTURES_FOLDER).Unwrap();
        }

        /// <summary>
        /// Upload profile picture for agent
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public async Task<Result> UploadProfilePictureAsync(IFormFile formFile, string resourceId) {
            return await UploadDocument(formFile, _profilePicturesFolder, ResourceType.Image, MAX_FILE_SIZE_PROFILEPICTURE, resourceId, true);
        }

        /// <summary>
        /// Upload profile picture for agent
        /// </summary>
        /// <param name="browserFile"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public async Task<Result> UploadProfilePictureAsync(IBrowserFile browserFile, string resourceId) {
            return await UploadDocument(browserFile, _profilePicturesFolder, ResourceType.Image, MAX_FILE_SIZE_PROFILEPICTURE, resourceId, true);
        }

        public string GetURIForProfilePicture(string resourceId) {
            var result = _resourcesService.GetUriFor(resourceId);
            if (result.IsFailed) {
                return string.Empty;
            }
            return result.Unwrap();
        }

        /// <summary>
        /// Upload document
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="folder">The folder to store the document</param>
        /// <param name="resourceType">The type of resource (image or document)</param>
        /// <param name="maxFileSize">The max file size permitted</param>
        /// <returns></returns>
        private async Task<Result> UploadDocument(IFormFile formFile, string folder, ResourceType resourceType, int maxFileSize, string resourceId, bool isPublic) {
            Result result = new();
            try {
                string fileName = formFile.FileName;
                _logger.LogInformation($"Uploading document to folder '{folder}' with name '{fileName}'");
                if (formFile == null || formFile.Length <= 0) {
                    _logger.LogWarning($"Failed to upload document '{fileName}' to folder '{folder}' because it is either null or 0kb");
                    return result
                        .Fail()
                        .WithError($"Failed to upload document '{fileName}' to folder '{folder}' because it is either null or 0kb");
                }

                //save image to resources.
                using var memoryStream = new MemoryStream();
                await formFile.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Upload the file if less than max size
                if (memoryStream.Length < maxFileSize) {
                    return await _resourcesService.AddResourceAsync(memoryStream, folder, fileName, resourceType, isPublic, resourceId);
                }
                else {
                    result.Fail().WithError("The file is too large.");
                }
            }
            catch (Exception ex) {
                ServicesHelper.HandleServiceError(ref result, _logger, ex, "Error while trying to upload document");
            }

            return result;
        }

        /// <summary>
        /// Upload document
        /// </summary>
        /// <param name="browserFile"></param>
        /// <param name="folder">The folder to store the document</param>
        /// <param name="resourceType">The type of resource (image or document)</param>
        /// <param name="maxFileSize">The max file size permitted</param>
        /// <returns></returns>
        private async Task<Result> UploadDocument(IBrowserFile browserFile, string folder, ResourceType resourceType, int maxFileSize, string resourceId, bool isPublic) {
            Result<string> result = new();
            try {
                string fileName = browserFile.Name;
                _logger.LogInformation($"Uploading document to folder '{folder}' with name '{fileName}'");
                if (browserFile == null || browserFile.Size <= 0) {
                    _logger.LogWarning($"Failed to upload document '{fileName}' to folder '{folder}' because it is either null or 0kb");
                    return result
                        .Fail()
                        .WithError($"Failed to upload document '{fileName}' to folder '{folder}' because it is either null or 0kb");
                }

                if (browserFile.Size > maxFileSize) {
                    result.Fail().WithError("The file is too large.");
                    return result;
                }

                //save image to resources.
                using var fileStream = browserFile.OpenReadStream(maxFileSize);

                // Upload the file if less than max size
                return await _resourcesService.AddResourceAsync(fileStream, folder, fileName, resourceType, isPublic, resourceId);
            }
            catch (Exception ex) {
                ServicesHelper.HandleServiceError(ref result, _logger, ex, "Error while trying to upload document");
            }

            return result;
        }
    }
}
