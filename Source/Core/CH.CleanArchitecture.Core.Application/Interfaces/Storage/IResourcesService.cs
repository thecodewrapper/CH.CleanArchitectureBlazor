using System.IO;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Common.Enumerations;
using CH.CleanArchitecture.Core.Application.DTOs;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IResourcesService
    {
        /// <summary>
        /// Adds a resource entity to store. Returns the resource id if successfull wrapped in a <see cref="Result"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="resourceName">The name of the resource</param>
        /// <param name="folderName"></param>
        /// <param name="isPublic">Whether this is a publicly accessible resource</param>
        /// <param name="type"></param>
        /// <returns>The created Resource URI wrapped in a <see cref="Result"/></returns>
        Task<Result<string>> AddResourceAsync(Stream stream, string folderName, string resourceName, ResourceType type, bool isPublic);

        /// <summary>
        /// Adds a resource entity to store. Returns the resource id if successfull wrapped in a <see cref="Result"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="resourceName">The name of the resource</param>
        /// <param name="folderName"></param>
        /// <param name="type"></param>
        /// <param name="isPublic">Whether this is a publicly accessible resource</param>
        /// <param name="resourceId">The ID of the resource</param>
        /// <returns>The created Resource URI wrapped in a <see cref="Result"/></returns>
        Task<Result> AddResourceAsync(Stream stream, string folderName, string resourceName, ResourceType type, bool isPublic, string resourceId);

        /// <summary>
        /// Returns the URI for the given resourceId
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns>URI for the given resourceId</returns>
        Result<string> GetUriFor(string resourceId);

        /// <summary>
        /// Deletes the resource from store
        /// </summary>
        /// <param name="resourceId">Primary key of the entity.</param>
        /// <param name="forceDelete">Indicates whether to delete the entity regardless if the deletion from the store was successful</param>
        /// <returns>True if the delete was successfull.</returns>
        Task<Result<bool>> DeleteResourceAsync(string resourceId, bool forceDelete);

        /// <summary>
        /// Downloads the resource from store
        /// </summary>
        /// <param name="resourceId">Primary key of the entity.</param>
        /// <returns>A stream representing the resouce.</returns>
        Task<Result<ResourceDTO>> DownloadResourceAsync(string resourceId);
    }
}
