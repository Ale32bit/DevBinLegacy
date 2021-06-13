using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DevBin.Pages {
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ErrorStatusCode { get; set; }
        public string ErrorDescription { get; set; }
        public string OriginalURL { get; set; }
        public bool ShowOriginalURL => !string.IsNullOrEmpty(OriginalURL);

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger) {
            _logger = logger;
        }

        public void OnGet(string code) {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            
            ErrorStatusCode = code;

            if (HttpContext.Request.RouteValues.ContainsKey("ErrorDescription"))
                ErrorDescription = (string) HttpContext.Request.RouteValues["ErrorDescription"];

            var statusCodeReExecuteFeature = HttpContext.Features.Get<
                IStatusCodeReExecuteFeature>();
            if (statusCodeReExecuteFeature != null)
                OriginalURL =
                    statusCodeReExecuteFeature.OriginalPathBase
                    + statusCodeReExecuteFeature.OriginalPath
                    + statusCodeReExecuteFeature.OriginalQueryString;
        }
    }
}