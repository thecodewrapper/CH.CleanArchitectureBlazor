using System.Collections.Generic;
using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Core.Application.ReadModels
{
    public class UserReadModel : IReadModel
    {
        public string ProfilePicture { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public IReadOnlyCollection<RoleEnum> Roles { get; set; }
        public string PhoneNumber { get; set; }
        public string SecondaryPhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string UICulture { get; set; }
        public AddressReadModel Address { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}