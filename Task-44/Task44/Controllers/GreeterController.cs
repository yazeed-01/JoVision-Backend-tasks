using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Task44.Controllers
{
    [Route("api/")]
    [ApiController]
    public class GreeterController : ControllerBase
    {
        [HttpGet("hello")]
        public ActionResult<string> Hello([FromQuery] string name = "anonymous")
        {
            name = name.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest( "The name field cannot be empty or contain only spaces");
            }

            if (!Regex.IsMatch(name, @"^[a-zA-Z]+$"))
            {
                return BadRequest( "The name must only contain letters");
            }

            return Ok($"Hello {name} 🥸");
        }
    }
}
