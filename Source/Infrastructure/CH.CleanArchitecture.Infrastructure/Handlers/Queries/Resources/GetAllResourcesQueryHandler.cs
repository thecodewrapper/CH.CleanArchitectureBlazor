using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CH.CleanArchitecture.Infrastructure.Handlers.Queries
{
    public class GetAllResourcesQueryHandler : BaseMessageHandler<GetAllResourcesQuery, Result<List<ResourceReadModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IResourcesService _resourcesService;

        public GetAllResourcesQueryHandler(IServiceProvider serviceProvider, IMapper mapper, IResourcesService resourcesService) : base(serviceProvider) {
            _mapper = mapper;
            _resourcesService = resourcesService;
        }

        public override async Task<Result<List<ResourceReadModel>>> HandleAsync(GetAllResourcesQuery query) {
            var result = new Result<List<ResourceReadModel>>();
            IQueryable<ResourceDTO> allEntities = _resourcesService.GetAll().Unwrap();

            var data = _mapper.ProjectTo<ResourceReadModel>(allEntities);

            int allRecordCount = await data.CountAsync();
            data = data.ApplyQueryOptions(query.Options, c => c.Name);

            result.Succeed().WithData(await data.ToListAsync());
            result.AddMetadata("RecordCount", data.Count());
            result.AddMetadata("AllRecordCount", allRecordCount);

            return result;
        }
    }
}
