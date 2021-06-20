using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.User {
    [IgnoreAntiforgeryToken(Order = 20000)]
    public class LoginModel : PageModel {
        public void OnGet() {
            if (HttpContext.Items.TryGetValue("logged_user", out var user)) {
                if (user != null) {
                    Response.Redirect("/");
                }
            }
        }

        public ActionResult OnPost() {
            if (!Request.HasFormContentType) {
                return BadRequest("Invalid Content-Type");
            }
            
            string loginName = Request.Form["email"];
            string password = Request.Form["password"];

            var user = Database.Instance.FetchUser(loginName);

            if (user == null || !user.PasswordMatch(password))
                return new JsonResult(new {
                    ok = false,
                    message = "Wrong password or user does not exist",
                });
            var token = user.GenerateSessionToken();
            return new JsonResult(new {
                ok = true,
                token = token,
            });
        }
    }
}