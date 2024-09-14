using System.Collections.Generic;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Core.Application.Queries
{
    public class GetAllOrdersQuery : BaseQuery<Result<List<OrderReadModel>>>
    {
    }
}