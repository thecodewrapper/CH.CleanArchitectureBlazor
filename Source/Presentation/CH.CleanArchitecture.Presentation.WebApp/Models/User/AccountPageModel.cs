using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CH.CleanArchitecture.Presentation.WebApp.Models
{
    [Authorize]
    public class AccountPageModel : PageModel
    {
    }
}
