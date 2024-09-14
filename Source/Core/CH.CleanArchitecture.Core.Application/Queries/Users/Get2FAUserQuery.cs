using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Core.Application.Queries
{
    public class Get2FAUserQuery : BaseQuery<Result<UserReadModel>>
    {
        public string Id { get; set; }
    }
}