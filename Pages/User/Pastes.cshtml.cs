using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.User {
    public class PastesModel : PageModel {
        [Route("/user/pastes/{user}")]
        public void UserPastes(string user) {
            Redirect("/");
        }
    }
}