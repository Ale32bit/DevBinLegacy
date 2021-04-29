using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin {
    public class PasteController : Controller {
        [HttpGet("{path:regexRouter}")]
        public IActionResult Index([FromRoute] string path) {
            return Ok(path);
        }
    }
}
