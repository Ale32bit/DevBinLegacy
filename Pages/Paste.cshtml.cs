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
            HttpContext.Request.RouteValues.TryGetValue("paste", out var paste);

            Paste = (Paste) paste;
            PasteExposure = Paste.Exposure switch {
                Paste.Exposures.Unlisted => "Unlisted",
                Paste.Exposures.Private => "Private",
                Paste.Exposures.Encrypted => "Encrypted",
                _ => "Public"
            };
            PasteContent = PasteFs.Instance.Read(Paste.ID);
        }
    }
}