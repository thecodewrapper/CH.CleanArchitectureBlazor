namespace CH.CleanArchitecture.Core.Application.DTOs
{
    public class Login2FARequestDTO
    {
        public string Code { get; set; }
        public bool IsPersisted { get; set; }
        public bool RememberClient { get; set; }
    }
}