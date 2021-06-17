using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using Org.BouncyCastle.Ocsp;
using static DevBin.Paste;

namespace DevBin.Pages {
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;

        public bool IsCloning { get; set; }
        public Paste Clone { get; set; } = null!;
        public string CloneContent { get; set; } = null!;

        public IndexModel(ILogger<IndexModel> logger) {
            _logger = logger;
        }

        public void OnGet() {
            if (HttpContext.Request.Query.ContainsKey("clone")) {
                Database database = HttpContext.RequestServices.GetService(typeof(Database)) as Database;
                PasteFs pasteFs = HttpContext.RequestServices.GetService(typeof(PasteFs)) as PasteFs;

                Clone = database.FetchPaste(HttpContext.Request.Query["clone"]);
                if (Clone != null) {
                    IsCloning = true;
                    CloneContent = pasteFs.Read(Clone.ID);
                }
            }
        }

        public void OnPost() {
            string content = Request.Form["paste-input"];
            Paste paste = new();
            paste.Title = "Unnamed Paste";
            if (Request.Form.ContainsKey("paste-title") && ((string) Request.Form["paste-title"]).Length > 0)
                paste.Title = Request.Form["paste-title"];

            paste.Syntax = "plaintext";
            if (Request.Form.ContainsKey("paste-syntax") && ((string) Request.Form["paste-syntax"]).Length > 0)
                paste.Syntax = Request.Form["paste-syntax"];

            paste.Exposure = Exposures.Public;
            if (int.TryParse(Request.Form["paste-exposure"].ToString(), out var exp))
                paste.Exposure = exp switch {
                    1 => Exposures.Unlisted,
                    2 => Exposures.Private,
                    3 => Exposures.Encrypted,
                    _ => Exposures.Public
                };

            var asGuest = !HttpContext.Items.ContainsKey("logged_user");
            bool.TryParse(Request.Form["paste-as-guest"].ToString(), out asGuest);
            
            paste.ContentCache = content.Substring(0, Math.Min(64, content.Length));

            paste.Date = DateTime.Now;

            var id = Database.Instance.Upload(paste, (!asGuest) ? (DevBin.User?)HttpContext.Items["logged_user"] : null);
            PasteFs.Instance.Write(id, content);

            HttpContext.Response.Redirect(id);
        }
    }
}