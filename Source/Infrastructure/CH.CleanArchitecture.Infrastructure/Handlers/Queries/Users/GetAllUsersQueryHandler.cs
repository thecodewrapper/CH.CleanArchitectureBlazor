﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Infrastructure.Handlers.Queries
{
    public class GetAllUsersQueryHandler : BaseMessageHandler<GetAllUsersQuery, Result<IEnumerable<UserReadModel>>>
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IMapper _mapper;

        public GetAllUsersQueryHandler(IApplicationUserService applicationUserService, IMapper mapper, IAuthenticatedUserService authenticatedUserService) {
            _applicationUserService = applicationUserService;
            _mapper = mapper;
            _authenticatedUserService = authenticatedUserService;
        }

        public override async Task<Result<IEnumerable<UserReadModel>>> HandleAsync(GetAllUsersQuery query) {
            var result = new Result<IEnumerable<UserReadModel>>();

            var getUsersResult = await _applicationUserService.GetAllUsersAsync(query.Options);

            if (getUsersResult.IsFailed)
                result.Fail();
            else {
                var data = _mapper.Map<IEnumerable<UserReadModel>>(getUsersResult.Data);

                if (query.ApplyRoleFilter) {
                    var currentUserHighestRole = _authenticatedUserService.Roles.Max();
                    data = data.Where(u => u.Roles.Max() <= currentUserHighestRole);
                }

                result.Succeed().WithData(data);
                result.AddMetadata("RecordCount", data.Count());
            }

            return result;
        }
    }
}