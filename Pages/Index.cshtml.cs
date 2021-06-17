using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.CodeAnalysis.Differencing;
using Org.BouncyCastle.Ocsp;
using static DevBin.Paste;

namespace DevBin.Pages {
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;

        public bool IsCloning { get; set; } = false;
        public bool IsEditing { get; set; } = false;
        public Paste? ContextPaste { get; set; }
        public string ContextContent { get; set; }

        public IndexModel(ILogger<IndexModel> logger) {
            _logger = logger;
        }

        public void OnGet() {
            if (HttpContext.Request.Query.ContainsKey("clone")) {
                ClonePaste();
            }
            else if (HttpContext.Request.Query.ContainsKey("edit")) {
                EditPaste();
            }
            else if (HttpContext.Request.Query.ContainsKey("delete")) {
                DeletePaste();
            }
        }

        private void DeletePaste() {
            ContextPaste = Database.Instance.FetchPaste(HttpContext.Request.Query["delete"]);
            if (ContextPaste == null) return;

            if (!HttpContext.Items.TryGetValue("logged_user", out var user)) return;
            if ((user as DevBin.User)?.ID == ContextPaste.AuthorID) {
                Database.Instance.Delete(ContextPaste);
                PasteFs.Instance.Delete(ContextPaste.ID);
            }
        }

        private void EditPaste() {
            ContextPaste = Database.Instance.FetchPaste(HttpContext.Request.Query["edit"]);
            if (ContextPaste == null) return;
            
            if (!HttpContext.Items.TryGetValue("logged_user", out var user)) return;
            if ((user as DevBin.User)?.ID != ContextPaste.AuthorID) {
                Redirect("/");
                return;
            }
            IsEditing = true;
            ContextContent = PasteFs.Instance.Read(ContextPaste.ID);
        }

        private void ClonePaste() {
            ContextPaste = Database.Instance.FetchPaste(HttpContext.Request.Query["clone"]);
            if (ContextPaste == null) return;
            IsCloning = true;
            ContextContent = PasteFs.Instance.Read(ContextPaste.ID);
        }

        public void OnPost() {
            string content = Request.Form["paste-input"];
            Paste paste = new() {Title = "Unnamed Paste"};
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


            var asGuest = true;
            if (HttpContext.Items.ContainsKey("logged_user")) {
                asGuest = Request.Form["paste-as-guest"].ToString() == "on";
            }

            paste.ContentCache = content[..Math.Min(64, content.Length)];
            paste.Date = DateTime.Now;

            string id;
            if (Request.Query.ContainsKey("edit")) {

                ContextPaste = Database.Instance.FetchPaste(HttpContext.Request.Query["edit"]);
                if (ContextPaste == null) return;
            
                if (!HttpContext.Items.TryGetValue("logged_user", out var user)) return;
                if ((user as DevBin.User)?.ID != ContextPaste.AuthorID) {
                    Redirect("/");
                    return;
                }

                id = ContextPaste.ID;
                Database.Instance.Update(paste);
            }
            else {
                id = Database.Instance.Upload(paste,
                    (!asGuest) ? (DevBin.User?) HttpContext.Items["logged_user"] : null);
            }

            PasteFs.Instance.Write(id, content);
            HttpContext.Response.Redirect(id);
        }
    }
}