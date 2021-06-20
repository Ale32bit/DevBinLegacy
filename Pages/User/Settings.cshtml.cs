using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.User {
    [IgnoreAntiforgeryToken(Order = 2000)]
    public class SettingsModel : PageModel {
        public ActionResult OnPost() {
            if (!Request.HasFormContentType) {
                return BadRequest("Invalid Content-Type");
            }

            var user = (DevBin.User?)HttpContext.Items["logged_user"];
            if (user == null) {
                return Redirect("/");
            }

            var form = Request.Form;

            if (form.ContainsKey("email")) { // change email
                string email = form["email"];

                if (DevBin.User.IsEmailValid(email)) {
                    var existingUser = Database.Instance.FetchUser(email);

                    if (existingUser != null) {
                        return new JsonResult(new {
                            ok = false,
                            message = "Email address already used",
                        });
                    }

                    if (Database.Instance.UpdateUserEmail(user, email)) {
                        return new JsonResult(new {
                            ok = true,
                            email = email,
                        });
                    }

                    return new JsonResult(new {
                        ok = false,
                        message = "Unknown error",
                    });
                }

                return new JsonResult(new {
                    ok = false,
                    message = "Invalid email address",
                });
            } else if (form.ContainsKey("old-password") && form.ContainsKey("new-password")) { // change password
                string oldPassword = form["old-password"];
                string newPassword = form["new-password"];

                if (!user.PasswordMatch(oldPassword)) {
                    return new JsonResult(new {
                        ok = false,
                        message = "Wrong password",
                    });
                }

                var hash = DevBin.User.Hash(newPassword);
                if (Database.Instance.UpdateUserPassword(user, hash)) {
                    return new JsonResult(new {
                        ok = true,
                    });
                }

                return new JsonResult(new {
                    ok = false,
                    message = "Unknown error"
                });
            } else if (form.ContainsKey("deletion-password")) { // delete account
                string password = form["deletion-password"];

                if (!user.PasswordMatch(password)) {
                    return new JsonResult(new {
                        ok = false,
                        message = "Wrong password",
                    });
                }

                user.DeleteUser(password);
                return new JsonResult(new {
                    ok = true,
                });
            }

            return BadRequest("Invalid request");
        }
    }
}