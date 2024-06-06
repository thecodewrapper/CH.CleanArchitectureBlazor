namespace CH.CleanArchitecture.Core.Application.Authorization
{
    /// <summary>
    /// Holds <see cref="UserOperationAuthorizationRequirement"/>s for user
    /// </summary>
    public static class UserOperations
    {
        public static UserOperationAuthorizationRequirement Create => new() { Name = OPERATIONS.USER.CREATE };
        public static UserOperationAuthorizationRequirement Read => new() { Name = OPERATIONS.USER.READ };
        public static UserOperationAuthorizationRequirement Update(string userId) => new(userId) { Name = OPERATIONS.USER.UPDATE };
        public static UserOperationAuthorizationRequirement Delete => new() { Name = OPERATIONS.USER.DELETE };
        public static UserOperationAuthorizationRequirement Activate => new() { Name = OPERATIONS.USER.ACTIVATE };
        public static UserOperationAuthorizationRequirement Deactivate => new() { Name = OPERATIONS.USER.DEACTIVATE };
        public static UserOperationAuthorizationRequirement ConfirmEmail(string userId) => new(userId) { Name = OPERATIONS.USER.CONFIRM_EMAIL };
    }
}
