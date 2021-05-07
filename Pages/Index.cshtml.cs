using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using static DevBin.Paste;

namespace DevBin.Pages {
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;

        public bool IsCloning { get; set; }
        public Paste Clone { get; set; }
        public string CloneContent { get; set; }

        public IndexModel(ILogger<IndexModel> logger) {
            _logger = logger;
        }

        public void OnGet() {
            if ( HttpContext.Request.Query.ContainsKey("clone") ) {
                Database database = HttpContext.RequestServices.GetService(typeof(Database)) as Database;
                PasteFs pasteFs = HttpContext.RequestServices.GetService(typeof(PasteFs)) as PasteFs;

                Clone = database.FetchPaste(HttpContext.Request.Query["clone"]);
                if ( Clone != null ) {
                    IsCloning = true;
                    CloneContent = pasteFs.Read(Clone.ID);
                }

            }
        }

        public void OnPost() {
            Paste paste = new();
            paste.Title = "Unnamed Paste";
            if ( Request.Form.ContainsKey("paste-title") && ((string)Request.Form["paste-title"]).Length > 0 ) {
                paste.Title = Request.Form["paste-title"];
            }

            paste.Syntax = "plaintext";
            if ( Request.Form.ContainsKey("paste-syntax") && ((string)Request.Form["paste-syntax"]).Length > 0 ) {
                paste.Syntax = Request.Form["paste-syntax"];
            }

            paste.Exposure = PasteExposure.Public;
            if ( int.TryParse(Request.Form["paste-exposure"].ToString(), out var exp) ) {
                paste.Exposure = exp switch {
                    1 => PasteExposure.Unlisted,
                    2 => PasteExposure.Private,
                    3 => PasteExposure.Encrypted,
                    _ => PasteExposure.Public,
                };
            }

            paste.Date = DateTime.Now;

            Database database = HttpContext.RequestServices.GetService(typeof(Database)) as Database;
            PasteFs pasteFs = HttpContext.RequestServices.GetService(typeof(PasteFs)) as PasteFs;

            var id = database.Upload(paste);

            string content = Request.Form["paste-input"];

            pasteFs.Write(id, content);

            HttpContext.Response.Redirect(id);
        }
    }
}
