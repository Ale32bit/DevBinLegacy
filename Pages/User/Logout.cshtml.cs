using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.User {
    public class LogoutModel : PageModel {
        public ActionResult OnGet() {
            if (HttpContext.Items.TryGetValue("logged_user", out var user)) {
                if (user != null) {
                    if (Request.Cookies.TryGetValue("devbin_session_token", out var token) && !string.IsNullOrEmpty(token)) {
                        Database.Instance.InvalidateSessionToken(token);
                        Response.Cookies.Delete("devbin_session_token");
                    }
                }
            }
            return Redirect("/");
        }
    }
}