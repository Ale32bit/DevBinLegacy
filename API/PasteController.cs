using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin.API {
    public class APIError {
        public int Code { get; set; }
        public string Message { get; set; }
        public APIError(int code, string message) {
            Code = code;
            Message = message;
        }
    }

    /// <summary>
    /// API to interact with pastes
    /// </summary>
    [ApiController]
    [Route("/api/v2/paste")]
    public class PasteController : Controller {
        /// <summary>
        /// Fetch a paste information using its ID
        /// </summary>
        /// <param name="id">Paste ID</param>
        /// <returns>Information about the paste</returns>
        /// <remarks>Returns an empty set for not existing pastes</remarks>
        [Route("/api/v2/paste/{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(Paste), 200)]
        [ProducesResponseType(typeof(APIError), 404)]
        [Produces("application/json")]
        public ActionResult Index(string id) {
            Database database = HttpContext.RequestServices.GetService(typeof(Database)) as Database;
            Paste paste = database.FetchPaste(id);


            if ( paste != null ) {
                return Ok(paste);
            }

            return NotFound(new APIError(404, "Paste not found"));
        }

        /// <summary>
        /// Upload a new paste
        /// </summary>
        [Route("/api/v2/create")]
        [HttpPost]
        [ProducesResponseType(typeof(Paste), 200)]
        [ProducesResponseType(typeof(APIError), 400)]
        [Produces("application/json")]
        [Consumes("application/json")]
        public ActionResult Create([FromBody]UserPaste userPaste) {

            if(!ModelState.IsValid) {
                return BadRequest(new APIError(400, "Invalid JSON data"));
            }


            if(userPaste.content == null || userPaste.title == null) {
                return BadRequest(new APIError(400, "Missing fields"));
            }


            Database database = HttpContext.RequestServices.GetService(typeof(Database)) as Database;
            PasteFs pasteFs = HttpContext.RequestServices.GetService(typeof(PasteFs)) as PasteFs;

            Paste paste = new Paste() {
                Title = userPaste.title,
                Exposure = (Paste.PasteExposure)userPaste.exposure,
                Syntax = userPaste.syntax ?? "plaintext",
                ContentCache = userPaste.content.Substring(0, Math.Min(userPaste.content.Length, 64)),
            };

            string id = database.Upload(paste);

            pasteFs.Write(id, userPaste.content);

            Paste cPaste = database.FetchPaste(id);

            return Ok(cPaste);
        }


    }
}
