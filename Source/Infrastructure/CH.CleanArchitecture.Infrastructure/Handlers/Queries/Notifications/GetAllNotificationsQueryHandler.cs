using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Infrastructure.Handlers.Queries
{
    public class GetAllNotificationsQueryHandler : BaseMessageHandler<GetAllNotificationsQuery, Result<List<NotificationReadModel>>>
    {
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public GetAllNotificationsQueryHandler(IMapper mapper, INotificationService notificationService) {
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
            var options = query.Options;
            if (!string.IsNullOrWhiteSpace(options?.SearchTerm)) {
                data = data.Where(c => EF.Functions.Like(c.Title.ToLower(), $"%{options.SearchTerm.ToLower()}%"));
            }

            //if (!string.IsNullOrWhiteSpace(options?.OrderBy)) {
            //    data = data.OrderBy(options.OrderBy);
            //}

            data = data.OrderByDescending(n => n.DateCreated);

            if (options?.Skip != null && options?.PageSize != null) {
                data = data.Skip(options.Skip);
                data = data.Take(options.PageSize);
            }

            result.Succeed().WithData(await data.ToListAsync());
            result.AddMetadata("RecordCount", data.Count());
            result.AddMetadata("AllRecordCount", allRecordCount);

            return result;
        }
    }
}
