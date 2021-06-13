using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevBin.API {
    [Route("api/v2/[controller]")]
    [ApiController]
    public class UserController : ControllerBase {
        /// <summary>
        /// Retrieves information about your account along with all your pastes
        /// </summary>
        /// <returns>Your profile information</returns>
        [Route("/api/v2/user")]
        [HttpGet]
        [ProducesResponseType(typeof(UserProfile), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.Unauthorized)]
        [Produces("application/json")]
        public ActionResult Index() {
            return Ok();
        }

        /// <summary>
        /// Retrieves information about an account
        /// </summary>
        /// <param name="username">User username</param>
        /// <returns>User profile with limited information</returns>
        [Route("/api/v2/user/{username}")]
        [HttpGet]
        [ProducesResponseType(typeof(UserProfileLimited), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(Response), (int) HttpStatusCode.NotFound)]
        [Produces("application/json")]
        public ActionResult Index(string username) {
            return Ok();
        }
    }
}