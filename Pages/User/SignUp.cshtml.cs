using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.User {
    public class SignUpModel : PageModel {
        public IActionResult OnGet() {
            return RedirectPermanent("/user/login");
        }

        public void OnPost() {
            
        }
    }
}