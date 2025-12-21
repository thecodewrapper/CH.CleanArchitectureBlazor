using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AutoMapper;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.DTOs.Notifications;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CH.CleanArchitecture.Infrastructure.Handlers.Queries
{
    public class GetAllNotificationsQueryHandler : BaseMessageHandler<GetAllNotificationsQuery, Result<List<NotificationReadModel>>>
    {
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public GetAllNotificationsQueryHandler(IServiceProvider serviceProvider, IMapper mapper, INotificationService notificationService) : base(serviceProvider) {
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public override async Task<Result<List<NotificationReadModel>>> HandleAsync(GetAllNotificationsQuery query) {
            var result = new Result<List<NotificationReadModel>>();
            IQueryable<NotificationDTO> allEntities;

            if (!string.IsNullOrEmpty(query.UserFor)) {
                allEntities = _notificationService.GetAllForUser(query.UserFor).Unwrap();
            }
            else {
                allEntities = _notificationService.GetAll().Unwrap();
            }

            if (query.Type.HasValue) {
                allEntities = allEntities.Where(n => n.Type == query.Type);
            }

            var data = _mapper.ProjectTo<NotificationReadModel>(allEntities);

            int allRecordCount = await data.CountAsync();
            data = data.ApplyQueryOptions(query.Options, c => c.Title);

            result.Succeed().WithData(await data.ToListAsync());
            result.AddMetadata("RecordCount", data.Count());
            result.AddMetadata("AllRecordCount", allRecordCount);

            return result;
        }
    }
}
