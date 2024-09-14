﻿using System;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Core.Application.Queries
{
    public class GetOrderByIdQuery : BaseQuery<Result<OrderReadModel>>
    {
        public Guid OrderId { get; private set; }

        public GetOrderByIdQuery(Guid orderId) {
            OrderId = orderId;
        }
    }
}
