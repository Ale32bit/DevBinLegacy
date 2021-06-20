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
            if (!Request.HasFormContentType) {
                return BadRequest("Invalid Content-Type");
            }
            
            string emailAddress = Request.Form["email"];
            string username = Request.Form["username"];
            string password = Request.Form["password"];

            Response.ContentType = MediaTypeNames.Application.Json;

            if (!DevBin.User.IsEmailValid(emailAddress))
                return new JsonResult(new {
                    ok = false,
                    message = "Invalid email address",
                });

            if (!DevBin.User.IsUsernameValid(username))
                return new JsonResult(new {
                    ok = false,
                    message = "Invalid username",
                });

            if (Database.Instance.FetchUser(emailAddress) != null || Database.Instance.FetchUser(username) != null) {
                return new JsonResult(new {
                    ok = false,
                    message = "User already exists",
                });
            }

            string hashedPassword = DevBin.User.Hash(password);
            
            DevBin.User newUser = new(hashedPassword) {
                Email = emailAddress,
                Username = username,
            };
            
            var user = Database.Instance.CreateUser(newUser, hashedPassword);

            var token = user?.GenerateSessionToken();

            return new JsonResult(new {
                ok = true,
                token = token,
            });
        }
    }
}