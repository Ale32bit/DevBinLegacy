using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DevBin.Pages {
    public class PasteModel : PageModel {
        private readonly ILogger<PasteModel> _logger;

        public Paste Paste { get; set; }
        public string PasteExposure { get; set; }
        public string PasteContent { get; set; }

        public PasteModel(ILogger<PasteModel> logger) {
            _logger = logger;
        }

        public void OnGet() {
            PasteFs pasteFs = HttpContext.RequestServices.GetService(typeof(PasteFs)) as PasteFs;

            HttpContext.Request.RouteValues.TryGetValue("paste", out var paste);

            Paste = (Paste)paste;
            PasteExposure = Paste.Exposure switch {
                Paste.PasteExposure.Unlisted => "Unlisted",
                Paste.PasteExposure.Private => "Private",
                Paste.PasteExposure.Encrypted => "Encrypted",
                _ => "Public",
            };
            PasteContent = pasteFs.Read(Paste.ID);

        }

    }
}
