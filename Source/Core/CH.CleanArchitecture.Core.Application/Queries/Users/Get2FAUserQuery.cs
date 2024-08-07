﻿using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Core.Application.Queries
{
    public class Get2FAUserQuery : IRequest<Result<UserReadModel>>, IQuery
    {
        public string Id { get; set; }
    }
}