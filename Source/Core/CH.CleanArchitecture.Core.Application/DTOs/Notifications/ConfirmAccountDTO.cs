using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Core.Application.DTOs.Notifications
{
    public class ConfirmAccountDTO
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string ConfirmationUrl { get; set; }
    }
}
