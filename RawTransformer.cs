using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace DevBin {
    public class RawTransformer : DynamicRouteValueTransformer {
        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext,
            RouteValueDictionary values) {
            return await Task.Run(() => {
                var pasteId = values["pasteId"] as string;

                var paste = Database.Instance.FetchPaste(pasteId);
                httpContext.Response.ContentType = "text/plain; charset=UTF-8";
                if (paste is {
                    Exposure: Paste.Exposures.Public or Paste.Exposures.Unlisted or Paste.Exposures.Encrypted
                }) {
                    string pasteContent = PasteFs.Instance.Read(paste.ID);
                    httpContext.Response.WriteAsync(pasteContent).Wait();
                }
                else {
                    httpContext.Response.StatusCode = 404;
                    httpContext.Response.WriteAsync("Error 404 - The paste could not be found").Wait();
                }

                return values;
            });
        }
    }
}