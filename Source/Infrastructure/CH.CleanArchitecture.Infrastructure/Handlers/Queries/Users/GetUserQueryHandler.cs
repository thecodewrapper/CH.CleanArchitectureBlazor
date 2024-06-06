﻿using System.Threading.Tasks;
using AutoMapper;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Infrastructure.Handlers.Queries
{
    public class GetUserQueryHandler : BaseMessageHandler<GetUserQuery, Result<UserReadModel>>
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly IMapper _mapper;

        public GetUserQueryHandler(IApplicationUserService applicationUserService, IMapper mapper) {
            _applicationUserService = applicationUserService;
            _mapper = mapper;
        }

        public override async Task<Result<UserReadModel>> HandleAsync(GetUserQuery query) {
            var result = new Result<UserReadModel>();
            var userResult = await _applicationUserService.GetUserByIdAsync(query.Id);

            if (userResult.IsFailed)
                result.Fail();
            else
                result.Succeed().WithData(_mapper.Map<UserReadModel>(userResult.Data));

            return result;
        }
    }
}