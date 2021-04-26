using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace devbin.Pages {
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string ErrorStatusCode { get; set; }
        public string OriginalURL { get; set; }
        public bool ShowOriginalURL => !string.IsNullOrEmpty(OriginalURL);

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger) {
            _logger = logger;
        }

        public void OnGet(string code) {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            ErrorStatusCode = code;

            var statusCodeReExecuteFeature = HttpContext.Features.Get<
                                                   IStatusCodeReExecuteFeature>();
            if ( statusCodeReExecuteFeature != null ) {
                OriginalURL =
                    statusCodeReExecuteFeature.OriginalPathBase
                    + statusCodeReExecuteFeature.OriginalPath
                    + statusCodeReExecuteFeature.OriginalQueryString;
            }

            _logger.LogWarning($"Error: {code} {OriginalURL}");
        }
    }
}
