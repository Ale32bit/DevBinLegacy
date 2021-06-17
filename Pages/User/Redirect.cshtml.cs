using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.User {
    [IgnoreAntiforgeryToken(Order = 2000)]
    public class RedirectLoginModel : PageModel {
        public ActionResult OnGet([FromQuery] string token) {
            if (string.IsNullOrEmpty(token)) {
                return Redirect("/");
            }

            HttpContext.Response.Cookies.Append("devbin_session_token", token, new CookieOptions {
                HttpOnly = true,
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Strict,
            });

            return Redirect("/");
        }
    }
}