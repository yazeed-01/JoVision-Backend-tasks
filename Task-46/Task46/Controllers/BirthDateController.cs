using Microsoft.AspNetCore.Mvc;
using System;

namespace Task46.Controllers
{
    [Route("api/")]
    [ApiController]
    public class BirthDateController : ControllerBase
    {
        [HttpPost("birthdate")]
        public ActionResult<string> CalculateAge([FromForm] string name, [FromForm] int year, [FromForm] int month, [FromForm] int day)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "anonymous";
            }
            if (year == 0 && month == 0 && day == 0)
            {
                return BadRequest($"Hello {name}, all birthdate values cannot be zero");
            }
            
            if (year < 0 || month < 0 || day < 0)
            {
                return BadRequest($"Hello {name}, birthdate values must be positive numbers");
            }

            DateTime today = DateTime.Today;
            DateTime birthDate;

            try
            {
                birthDate = new DateTime(year, month, day);
            }
            catch (Exception)
            {
                return BadRequest($"Hello {name}, the provided birthdate is invalid");
            }

            int age = today.Year - birthDate.Year;
            if (today < birthDate.AddYears(age))
            {
                age--;
            }

            return Ok($"Hello {name}, your age is {age} years");
        }
    }
}
