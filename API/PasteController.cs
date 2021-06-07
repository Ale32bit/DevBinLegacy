using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DevBin.API {
    /// <summary>
    /// API to interact with pastes
    /// </summary>
    [Route("api/v2/[controller]")]
    [ApiController]
    public class PasteController : Controller {
        /// <summary>
        /// Fetch a paste information using its ID
        /// </summary>
        /// <param name="id">Paste ID</param>
        /// <returns>Information about the paste</returns>
        /// <remarks>Paste content can be fetched from /raw/{id}</remarks>
        [Route("/api/v2/paste/{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(Paste), (int) HttpStatusCode.Created)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.NotFound)]
        [Produces("application/json")]
        public ActionResult Index(string id) {
            Database database = HttpContext.RequestServices.GetService(typeof(Database)) as Database;
            Paste paste = database.FetchPaste(id);


            if (paste != null) {
                return Ok(paste);
            }

            return NotFound(new Response(404, "Paste not found", false));
        }

        /// <summary>
        /// Upload a paste
        /// </summary>
        /// <param name="userPaste">Paste data</param>
        /// <returns></returns>
        [Route("/api/v2/create")]
        [HttpPost]
        [ProducesResponseType(typeof(Paste), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.Unauthorized)]
        [Produces("application/json")]
        [Consumes("application/json")]
        public ActionResult Create([FromBody] UserPaste userPaste) {
            if (!ModelState.IsValid) {
                return BadRequest(new Response(400, "Invalid JSON data", false));
            }


            if (userPaste.Content == null || userPaste.Title == null) {
                return BadRequest(new Response(400, "Missing fields", false));
            }

            if (userPaste.AsGuest) {
                if (userPaste.Exposure == Paste.Exposures.Private || userPaste.Exposure == Paste.Exposures.Encrypted) {
                    userPaste.Exposure = Paste.Exposures.Unlisted;
                }
            }

            Database database = HttpContext.RequestServices.GetService(typeof(Database)) as Database;
            PasteFs pasteFs = HttpContext.RequestServices.GetService(typeof(PasteFs)) as PasteFs;

            Paste paste = new() {
                Title = userPaste.Title,
                Exposure = userPaste.Exposure,
                Syntax = userPaste.Syntax ?? "plaintext",
                ContentCache = userPaste.Content.Substring(0, Math.Min(userPaste.Content.Length, 64)),
            };

            string id = database.Upload(paste);

            pasteFs.Write(id, userPaste.Content);

            Paste cPaste = database.FetchPaste(id);

            return Ok(cPaste);
        }

        /// <summary>
        /// Update information or content of an own paste
        /// </summary>
        /// <param name="id">Paste ID</param>
        /// <param name="userPaste">New paste data</param>
        /// <returns>Updated paste</returns>
        [Route("/api/v2/update/{id}")]
        [HttpPut]
        [ProducesResponseType(typeof(Paste), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.Unauthorized)]
        [Produces("application/json")]
        [Consumes("application/json")]
        public ActionResult Update(string id, [FromBody] UserPaste userPaste) {
            return NotFound();
        }

        /// <summary>
        /// Delete a paste of yours
        /// </summary>
        /// <param name="id">Paste ID</param>
        /// <returns>The old paste information</returns>
        [Route("/api/v2/delete/{id}")]
        [HttpGet]
        [HttpDelete]
        [ProducesResponseType(typeof(Paste), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.Unauthorized)]
        [Produces("application/json")]
        public ActionResult Delete(string id) {
            return NotFound();
        }

        /// <summary>
        /// Get latest public pastes
        /// </summary>
        /// <param name="amount">Amount of pastes to fetch. Max is 50</param>
        /// <returns>List of public pastes</returns>
        [Route("/api/v2/latest")]
        [HttpGet]
        [ProducesResponseType(typeof(Paste[]), 200)]
        [Produces("application/json")]
        public ActionResult Latest(int amount = 30) {
            amount = Math.Min(amount, 50);
            Database database = HttpContext.RequestServices.GetService(typeof(Database)) as Database;
            return Ok(database.GetLatest(amount));
        }
    }
}