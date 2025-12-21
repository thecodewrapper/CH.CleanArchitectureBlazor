namespace CH.CleanArchitecture.Core.Application.Api
{
    public sealed record Unit
    {
        public static readonly Unit Value = new();
        private Unit() { }
    }
}
