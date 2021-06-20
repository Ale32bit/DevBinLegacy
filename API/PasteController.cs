using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
            Console.WriteLine(Request.Headers);
            
            var paste = Database.Instance.FetchPaste(id);
            
            if (paste != null) return Ok(paste);

            return NotFound(new Response(404, "Paste not found", false));
        }

        /// <summary>
        /// Upload a paste
        /// </summary>
        /// <param name="userPaste">Paste data</param>
        /// <param name="token">API Token</param>
        /// <returns></returns>
        [Route("/api/v2/create")]
        [HttpPost]
        [ProducesResponseType(typeof(Paste), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.Unauthorized)]
        [Produces("application/json")]
        [Consumes("application/json")]
        public ActionResult Create([FromBody] UserPaste userPaste, [FromQuery][Required] string token) {
            var tokenOwner = Database.Instance.ResolveApiToken(token);
            if (tokenOwner == null) {
                return Unauthorized(new Response(401, "Invalid token", false));
            }
            
            if (!ModelState.IsValid) return BadRequest(new Response(400, "Invalid JSON data", false));
            
            if (string.IsNullOrEmpty(userPaste.Content) ||  string.IsNullOrEmpty(userPaste.Title))
                return BadRequest(new Response(400, "Missing fields", false));

            if (userPaste.AsGuest)
                if (userPaste.Exposure is Paste.Exposures.Private or Paste.Exposures.Encrypted)
                    userPaste.Exposure = Paste.Exposures.Unlisted;

            Paste paste = new() {
                Title = userPaste.Title,
                Exposure = userPaste.Exposure ?? Paste.Exposures.Public,
                Syntax = userPaste.Syntax ?? "plaintext",
                ContentCache = userPaste.Content[..Math.Min(userPaste.Content.Length, 64)],
                Author = tokenOwner.Username,
                AuthorID = tokenOwner.ID,
            };

            string id = Database.Instance.Upload(paste);
            PasteFs.Instance.Write(id, userPaste.Content);
            var cPaste = Database.Instance.FetchPaste(id);

            return Ok(cPaste);
        }

        /// <summary>
        /// Update information or content of an own paste
        /// </summary>
        /// <param name="id">Paste ID</param>
        /// <param name="userPaste">New paste data</param>
        /// <param name="token">API Token</param>
        /// <returns>Updated paste</returns>
        [Route("/api/v2/update/{id}")]
        [HttpPost]
        [ProducesResponseType(typeof(Paste), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.Unauthorized)]
        [Produces("application/json")]
        [Consumes("application/json")]
        public ActionResult Update(string id, [FromBody] UserPaste userPaste, [FromQuery][Required] string token) {
            var tokenOwner = Database.Instance.ResolveApiToken(token);
            if (tokenOwner == null) {
                return Unauthorized(new Response(401, "Invalid token", false));
            }
            
            var paste = Database.Instance.FetchPaste(id);
            if (paste == null) {
                return NotFound(new Response(404, "Paste not found", false));
            }

            if (paste.AuthorID != tokenOwner.ID) {
                return Unauthorized(new Response(401, "Not your paste", false));
            }

            paste.Exposure = userPaste.Exposure ?? paste.Exposure;
            paste.Syntax = userPaste.Syntax ?? paste.Syntax;
            paste.Title = userPaste.Title ?? paste.Title;
            paste.ContentCache = userPaste.Content?[..Math.Min(64, userPaste.Content.Length)] ?? paste.ContentCache;

            Database.Instance.Update(paste);
            if (userPaste.Content != null) {
                PasteFs.Instance.Write(paste.ID, userPaste.Content);
            }

            return Ok(new Response(200, paste.ID, true));
        }

        /// <summary>
        /// Delete a paste of yours
        /// </summary>
        /// <param name="id">Paste ID</param>
        /// <param name="token">API Token</param>
        /// <returns>The old paste information</returns>
        [Route("/api/v2/delete/{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(Paste), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.Unauthorized)]
        [Produces("application/json")]
        public ActionResult Delete(string id, [FromQuery][Required] string token) {
            var tokenOwner = Database.Instance.ResolveApiToken(token);
            if (tokenOwner == null) {
                return Unauthorized(new Response(401, "Invalid token", false));
            }
            
            var paste = Database.Instance.FetchPaste(id);
            if (paste == null) {
                return NotFound(new Response(404, "Paste not found", false));
            }

            if (paste.AuthorID != tokenOwner.ID) {
                return Unauthorized(new Response(401, "Not your paste", false));
            }

            Database.Instance.Delete(paste);
            PasteFs.Instance.Delete(paste.ID);

            return Ok(new Response(200, paste.ID, true));
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
            return Ok(Database.Instance.GetLatest(amount));
        }
    }
}