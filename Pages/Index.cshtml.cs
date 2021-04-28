using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using static DevBin.Paste;

namespace DevBin.Pages {
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;

        public bool IsPost;

        public IndexModel(ILogger<IndexModel> logger) {
            _logger = logger;
        }

        public void OnGet() {
        }

        public void OnPost() {
            IsPost = true;
            Paste paste = new();
            
            paste.Title = Request.Form.ContainsKey("paste-title") ? Request.Form["paste-title"] : "Unnamed Paste";
            paste.Syntax = Request.Form.ContainsKey("paste-syntax") ? Request.Form["paste-syntax"] : "plaintext";
            paste.Exposure = PasteExposure.Public;
            if ( int.TryParse(Request.Form["paste-exposure"].ToString(), out var exp) ) {
                switch(exp) {
                    case 0:
                    default:
                        paste.Exposure = PasteExposure.Public;
                        break;
                    case 1:
                        paste.Exposure = PasteExposure.Unlisted;
                        break;
                    case 2:
                        paste.Exposure = PasteExposure.Private;
                        break;
                    case 3:
                        paste.Exposure = PasteExposure.Encrypted;
                        break;
                }
            }

            paste.Date = DateTime.Now;



            string content = Request.Form["paste-input"];
        }
    }
}
