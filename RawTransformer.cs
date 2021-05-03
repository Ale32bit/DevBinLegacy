using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace DevBin {
    public class RawTransformer : DynamicRouteValueTransformer {
        public RawTransformer() {

        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values) {
            return await Task.Run(() => {
                string pasteId = (string)values["pasteId"];

                Database database = httpContext.RequestServices.GetService(typeof(Database)) as Database;
                PasteFs pasteFs = httpContext.RequestServices.GetService(typeof(PasteFs)) as PasteFs;

                Paste? paste = database.FetchPaste(pasteId);

                Console.WriteLine("reached");

                if ( paste != null && paste.Exposure == Paste.PasteExposure.Public || paste.Exposure == Paste.PasteExposure.Unlisted ) {
                    string pasteContent = pasteFs.Read(paste.ID);
                    httpContext.Response.ContentType = "text/plain; charset=UTF-8";
                    httpContext.Response.WriteAsync(pasteContent).Wait();
                } else {
                    values["page"] = "/Error";
                }


                return values;
            });
        }
    }
}
