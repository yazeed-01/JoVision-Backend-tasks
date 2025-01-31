using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Task45.Controllers
{
    [Route("api/")]
    [ApiController]
    public class GreeterController : ControllerBase
    {
        [HttpGet("hello")]
        public ActionResult<string> Hello([FromQuery] string name = "anonymous")
        {
            name = name.Trim();

            // (check for empty or whitespace)
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { error = new[] { "The name field cannot be empty or contain only spaces" } });
            }

            // (check if it contains only letters)
            if (!Regex.IsMatch(name, @"^[a-zA-Z]+$"))
            {
                return BadRequest(new { error = new[] { "The name must only contain letters" } });
            }

            return Ok($"Hello {name} 🥸");
        }
    }
}
