namespace CH.CleanArchitecture.Core.Application.Constants
{
    public static class ApplicationClaimTypes
    {
        public static class User
        {
            public const string MustChangePassword = "user:mustchangepassword";
            public const string Username = "user:name";
            public const string FullName = "user:fullname";
            public const string ProfilePicture = "user:avatar";
            public const string Theme = "user:theme";
            public const string Culture = "localization:culture";
            public const string UiCulture = "localization:uiculture";
            public const string Id = "user:id";
        }

        public static class Client
        {
            public const string Id = "client:id";
            public const string Active = "client:active";
            public const string Email = "client:email";
            public const string Origin = "client:origin";
            public const string Name = "client:name";
            public const string SignInMethod = "client:signinmethod";
        }
    }
}
