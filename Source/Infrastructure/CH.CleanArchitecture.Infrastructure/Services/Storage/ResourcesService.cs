using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Common.Enumerations;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Infrastructure.Models;
using CH.Data.Abstractions;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class ResourcesService : IResourcesService
    {
        #region Private Fields

        private readonly ILogger<ResourcesService> _logger;
        private readonly IEntityRepository<ResourceEntity, string> _resourceRepository;
        private readonly IResourceStore _resourceStore;
        private readonly IMapper _mapper;

        #endregion Private Fields

        #region Public Constructors

        public ResourcesService(ILogger<ResourcesService> logger, IEntityRepository<ResourceEntity, string> resourceRepository, IResourceStore resourceStore, IMapper mapper) {
            _logger = logger;
            _resourceRepository = resourceRepository;
            _resourceStore = resourceStore;
            _mapper = mapper;
        }

        #endregion Public Constructors

        #region Public Methods

        public Result<IQueryable<ResourceDTO>> GetAll() {
            Result<IQueryable<ResourceDTO>> result = new Result<IQueryable<ResourceDTO>>();
            try {
                var resources = _resourceRepository.GetAll();
                result.Data = _mapper.ProjectTo<ResourceDTO>(resources);
                result.Succeed();
            }
            catch (Exception ex) {
                ServicesHelper.HandleServiceError(ref result, _logger, ex, "Error while trying to fetch all resources from the database");
            }

            return result;
        }

        /// <summary>
        /// Adds a resource entity to DB and saves the passed in stream in store. Returns the resource id if successful wrapped in a ServiceResult
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="resourceName"></param>
        /// <param name="folderName"></param>
        /// <param name="type"></param>
        /// <returns>The resource URI of the created resource</returns>
        public async Task<Result<string>> AddResourceAsync(Stream stream, string folderName, string resourceName, ResourceType type, bool isPublic) {
            Result<string> result = new Result<string>();
            try {
                string resourceId = await _resourceStore.SaveResourceAsync(stream, folderName, isPublic);
                _logger.LogDebug($"Resource {resourceId} saved on store. Attempting to add related resource entity...");

                string resourceURI = _resourceStore.GetResourceURI(folderName, resourceId);
                resourceName.TryGetFileExtension(out string extension);
                ResourceEntity resourceEntity = new ResourceEntity()
                {
                    Id = resourceId,
                    Name = resourceName.RemoveFileExtension(),
                    ContainerName = folderName,
                    Type = type,
                    URI = resourceURI,
                    Extension = extension
                };
                await _resourceRepository.AddAsync(resourceEntity);
                await _resourceRepository.UnitOfWork.SaveChangesAsync();
                _logger.LogDebug($"Resource entity {resourceId} created successfully");
                result.Data = resourceId;
                result.Succeed();
            }
            catch (Exception ex) {
                ServicesHelper.HandleServiceError(ref result, _logger, ex, "Error while trying to add resource");
            }
            return result;
        }

        /// <summary>
        /// Adds a resource entity to DB and saves the passed in stream in store. Returns the resource id if successful wrapped in a ServiceResult
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="resourceName"></param>
        /// <param name="folderName"></param>
        /// <param name="type"></param>
        /// <param name="resourceId"></param>
        /// <returns>The resource URI of the created resource</returns>
        public async Task<Result> AddResourceAsync(Stream stream, string folderName, string resourceName, ResourceType type, bool isPublic, string resourceId) {
            Result result = new Result();
            try {
                await _resourceStore.SaveResourceAsync(stream, folderName, isPublic, resourceId);
                _logger.LogDebug($"Resource {resourceId} saved on store. Attempting to add related resource entity...");

                string resourceURI = _resourceStore.GetResourceURI(folderName, resourceId);
                resourceName.TryGetFileExtension(out string extension);
                ResourceEntity resourceEntity = new ResourceEntity()
                {
                    Id = resourceId,
                    Name = resourceName.RemoveFileExtension(),
                    ContainerName = folderName,
                    Type = type,
                    URI = resourceURI,
                    Extension = extension
                };
                await _resourceRepository.AddAsync(resourceEntity);
                await _resourceRepository.UnitOfWork.SaveChangesAsync();
                _logger.LogDebug($"Resource entity {resourceId} created successfully");
                result.Succeed();
            }
            catch (Exception ex) {
                ServicesHelper.HandleServiceError(ref result, _logger, ex, "Error while trying to add resource");
            }
            return result;
        }

        /// <summary>
        /// Deletes the resource from DB and store
        /// </summary>
        /// <param name="resourceId">Primary key of the entity.</param>
        /// <param name="forceDelete">Indicates whether to delete the entity regardless if the deletion from the store was successful</param>
        /// <returns>True if the delete was successful.</returns>
        public async Task<Result<bool>> DeleteResourceAsync(string resourceId, bool forceDelete) {
            Result<bool> result = new Result<bool>();
            try {
                var resourceToDelete = _resourceRepository.GetSingle(r => r.Id == resourceId);
                if (resourceToDelete == null) {
                    throw new Exception("Resource not found");
                }

                bool deletedFromStore = await _resourceStore.DeleteResourceAsync(resourceToDelete.ContainerName, resourceId);
                if (!deletedFromStore && !forceDelete) {
                    _logger.LogError($"Resource {resourceId} failed to delete from store. Perhaps it doesn't exist.");
                    result.Fail().WithError("Failed to delete resource from store");
                    return result;
                }
                _logger.LogDebug($"Resource {resourceId} deleted from store ({deletedFromStore}). Attempting to remove related resource entity...");

                _resourceRepository.Delete(resourceToDelete);
                await _resourceRepository.UnitOfWork.SaveChangesAsync();

                _logger.LogDebug($"Resource entity {resourceId} removed successfully");
                result.Data = true;
            }
            catch (Exception ex) {
                ServicesHelper.HandleServiceError(ref result, _logger, ex, "Error while trying to delete resource");
            }
            return result;
        }

        public async Task<Result<ResourceDTO>> DownloadResourceAsync(string resourceId) {
            Result<ResourceDTO> result = new Result<ResourceDTO>();
            try {
                var resource = _resourceRepository.GetSingle(r => r.Id == resourceId);
                if (resource == null) {
                    throw new Exception("Resource not found");
                }

                ResourceDTO dto = _mapper.Map<ResourceDTO>(resource);
                using Stream resourceStreamFromStore = await _resourceStore.DownloadResourceAsync(resource.ContainerName, resourceId);
                resourceStreamFromStore.Position = 0;
                dto.Data = await BinaryData.FromStreamAsync(resourceStreamFromStore);

                _logger.LogDebug($"Resource data for resource '{resourceId}' retrieved from store.");
                result.Data = dto;
                result.Succeed();
            }
            catch (Exception ex) {
                ServicesHelper.HandleServiceError(ref result, _logger, ex, "Error while trying to delete resource");
            }
            return result;
        }

        /// <summary>
        /// Returns the URI for the given resourceId
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns>URI for the given resourceId</returns>
        public Result<string> GetUriFor(string resourceId) {
            Result<string> result = new Result<string>();
            try {
                result.Data = _resourceRepository.GetSingle(r => r.Id == resourceId)?.URI ?? string.Empty;
                result.Succeed();
            }
            catch (Exception ex) {
                ServicesHelper.HandleServiceError(ref result, _logger, ex, "Error while trying to fetch data from the database");
            }
            return result;
        }

        #endregion Public Methods
    }
}
