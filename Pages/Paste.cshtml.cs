using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using static DevBin.Paste;

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
            switch ( Paste.Exposure ) {
                default:
                case Paste.PasteExposure.Public:
                    PasteExposure = "Public";
                    break;
                case Paste.PasteExposure.Unlisted:
                    PasteExposure = "Unlisted";
                    break;
                case Paste.PasteExposure.Private:
                    PasteExposure = "Private";
                    break;
                case Paste.PasteExposure.Encrypted:
                    PasteExposure = "Encrypted";
                    break;
            }

            PasteContent = pasteFs.Read(Paste.ID);

        }

    }
}
