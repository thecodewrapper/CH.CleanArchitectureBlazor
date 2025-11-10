namespace CH.CleanArchitecture.Common
{
    public static class CommonResultErrors
    {
        public static readonly ResultError Unauthorized = new("Unauthorized", "1");
        public static readonly ResultError NotFound = new("NotFound", "2");
    }
}
