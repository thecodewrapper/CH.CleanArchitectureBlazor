using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetBox.Extensions;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.Interfaces.Storage;

namespace CH.CleanArchitecture.Presentation.WebApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("file")]
    public class FileController : Controller
    {
        private readonly ILogger<FileController> _logger;
        private readonly IResourcesService _resourcesService;

        public FileController(ILogger<FileController> logger, IResourcesService resourcesService) {
            _logger = logger;
            _resourcesService = resourcesService;
        }

        [HttpGet]
        [Route("download/{id}")]
        public async Task<IActionResult> Download(string id) {
            _logger.LogInformation($"Downloading file with id {id}");
            var fileResult = await _resourcesService.DownloadResourceAsync(id);
            if (fileResult.IsSuccessful) {
                ResourceDTO dto = fileResult.Unwrap();
                return File(dto.Data.ToArray(), "application/force-download", $"{dto.Name}{dto.Extension}");
            }
            else {
                return NotFound();
            }
        }
    }
}
