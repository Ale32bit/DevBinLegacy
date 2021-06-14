using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.User {
    [IgnoreAntiforgeryToken(Order = 20000)]
    public class SignUpModel : PageModel {
        public ActionResult OnGet() {
            return RedirectPermanent("/user/login");
        }

        public ActionResult OnPost() {
            string emailAddress = Request.Form["email"];
            string username = Request.Form["username"];
            string password = Request.Form["password"];

            Response.ContentType = MediaTypeNames.Application.Json;

            if (!DevBin.User.IsEmailValid(emailAddress))
                return new JsonResult(new API.Response(400, "Invalid email address", false));

            if (!DevBin.User.IsUsernameValid(username))
                return new JsonResult(new API.Response(400, "Invalid username", false));

            var database = HttpContext.RequestServices.GetService(typeof(Database)) as Database;
            if (database.FetchUser(emailAddress) != null || database.FetchUser(username) != null) {
                return new JsonResult(new API.Response(400, "User already exists", false));
            }

            string hashedPassword = DevBin.User.Hash(password);
            
            DevBin.User newUser = new(hashedPassword) {
                Email = emailAddress,
                Username = username,
            };
            
            var user = database.CreateUser(newUser, hashedPassword);

            var token = user.GenerateSessionToken();

            return new JsonResult(new API.Response(200, token, true));
        }
    }
}