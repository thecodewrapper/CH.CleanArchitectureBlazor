using System;
using System.Collections.Generic;
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
        private readonly IIdentityContext _identityContext;
        private readonly IMapper _mapper;

        public GetAllUsersQueryHandler(IServiceProvider serviceProvider, IApplicationUserService applicationUserService, IMapper mapper, IIdentityContext identityContext) : base(serviceProvider) {
            _applicationUserService = applicationUserService;
            _mapper = mapper;
            _identityContext = identityContext;
        }

        public override async Task<Result<IEnumerable<UserReadModel>>> HandleAsync(GetAllUsersQuery query) {
            var result = new Result<IEnumerable<UserReadModel>>();

            var getUsersResult = await _applicationUserService.GetAllUsersAsync(query.Options);

            if (getUsersResult.IsFailed)
                result.Fail();
            else {
                var data = _mapper.Map<IEnumerable<UserReadModel>>(getUsersResult.Data);

                result.Succeed().WithData(data);
                result.AddMetadata("RecordCount", data.Count());
                result.AddMetadata("AllRecordCount", data.Count());
            }

            return result;
        }
    }
}