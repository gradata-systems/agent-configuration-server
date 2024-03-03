using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ACS.Admin.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult LogOut()
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "/",
            }, "Cookies", "OpenIdConnect");
        }
    }
}
