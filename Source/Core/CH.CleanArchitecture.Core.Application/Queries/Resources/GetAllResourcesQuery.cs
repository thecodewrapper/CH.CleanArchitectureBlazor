using System.Collections.Generic;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Core.Application.Queries
{
    public class GetAllResourcesQuery : BaseQuery<Result<List<ResourceReadModel>>>
    {
        public QueryOptions Options { get; set; }
    }
}
