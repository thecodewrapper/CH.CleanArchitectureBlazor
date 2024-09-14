using System.Collections.Generic;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Core.Application.Queries
{
    public class GetAllUsersQuery : BaseQuery<Result<IEnumerable<UserReadModel>>>
    {
        public QueryOptions Options { get; set; }
    }
}