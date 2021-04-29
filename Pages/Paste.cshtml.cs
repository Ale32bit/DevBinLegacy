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

        public bool IsPost;

        public PasteModel(ILogger<PasteModel> logger) {
            _logger = logger;
        }

        public void OnGet() {
        }

    }
}
