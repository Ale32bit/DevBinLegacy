using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.User {
    [IgnoreAntiforgeryToken(Order = 20000)]
    public class LoginModel : PageModel {
        public void OnGet() { }

        public ActionResult OnPost() {
            string loginName = Request.Form["email"];
            string password = Request.Form["password"];

            var database = HttpContext.RequestServices.GetService(typeof(Database)) as Database;
            var user = database.FetchUser(loginName);

            if (user == null || !user.PasswordMatch(password))
                return new JsonResult(new API.Response(400, "Wrong password or user does not exist", false));
            var token = user.GenerateSessionToken();
            return new JsonResult(new API.Response(200, token, true));

        }
    }
}