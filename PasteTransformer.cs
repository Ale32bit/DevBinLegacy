using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace DevBin {
    public class PasteTransformer : DynamicRouteValueTransformer {
        public PasteTransformer() {

        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values) {
            return await Task.Run(() => {
                string pasteId = (string)values["pasteId"];

                Database database = httpContext.RequestServices.GetService(typeof(Database)) as Database;

                Paste paste = database.FetchPaste(pasteId);

                values.Remove("ErrorDescription");

                if ( paste != null ) {
                    values["paste"] = paste;
                    values["page"] = "/Paste";
                } else {
                    values["ErrorDescription"] = "Paste not found";
                    values["page"] = "/Error";
                    httpContext.Response.StatusCode = 404;
                }


                return values;
            });
        }
    }
}
