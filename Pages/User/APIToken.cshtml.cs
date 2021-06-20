using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.User {
    [IgnoreAntiforgeryToken(Order = 2000)]
    public class APITokenModel : PageModel {
        public ActionResult OnGet() {
            return RedirectPermanent("/user/settings");
        }

        public ActionResult OnPost() {
            if (!HttpContext.Items.ContainsKey("logged_user")) {
                return Redirect("/");
            }
            
            var user = (DevBin.User) HttpContext.Items["logged_user"]!;
            
            if (Request.Query.ContainsKey("delete")) {
                Database.Instance.DeleteToken(user);

                return new JsonResult(new {
                    ok = true,
                });
            } else if (Request.Query.ContainsKey("generate")) {
                var token = Database.Instance.GenerateToken(user);

                return new JsonResult(new {
                    ok = true,
                    token = token,
                });
            } else if (Request.Query.ContainsKey("fetch")) {
                var token = Database.Instance.GetUserApiToken(user);
                if (token != null) {
                    return new JsonResult(new {
                        ok = true,
                        token = token,
                    });
                }
                else {
                    return new JsonResult(new {
                        ok = false,
                    });
                }
            }

            return BadRequest();
        }
    }
}