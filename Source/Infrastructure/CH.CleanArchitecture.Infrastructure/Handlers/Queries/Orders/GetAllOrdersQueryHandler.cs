using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Infrastructure.DbContexts;
using CH.CleanArchitecture.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CH.CleanArchitecture.Infrastructure.Handlers.Queries
{
    public class GetAllOrdersQueryHandler : BaseMessageHandler<GetAllOrdersQuery, Result<List<OrderReadModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;

        public GetAllOrdersQueryHandler(IServiceProvider serviceProvider, IMapper mapper, ApplicationDbContext dbContext) : base(serviceProvider) {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public override async Task<Result<List<OrderReadModel>>> HandleAsync(GetAllOrdersQuery query) {
            var result = new Result<List<OrderReadModel>>();
            var orders = _dbContext.Orders.Include(o => o.OrderItems).AsNoTracking();

            var data = _mapper.ProjectTo<OrderReadModel>(orders);

            int allRecordCount = await data.CountAsync();

            data = data.ApplyQueryOptions(query.Options, e => e.TrackingNumber);

            var dataList = await data.ToListAsync();

            result.Succeed().WithData(dataList);
            result.AddMetadata("RecordCount", dataList.Count);
            result.AddMetadata("AllRecordCount", allRecordCount);

            return result;
        }
    }
}
