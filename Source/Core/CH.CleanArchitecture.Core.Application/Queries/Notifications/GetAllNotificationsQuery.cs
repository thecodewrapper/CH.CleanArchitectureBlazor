using CH.Messaging.Abstractions;
using System.Collections.Generic;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Core.Application.Queries
{
    public class GetAllNotificationsQuery : IRequest<Result<List<NotificationReadModel>>>, IQuery
    {
        public NotificationType? Type { get; set; }
        public string UserFor { get; set; }
        public QueryOptions Options { get; set; }
    }
}
