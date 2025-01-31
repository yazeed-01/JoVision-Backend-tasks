using Microsoft.AspNetCore.Mvc;
using System;

namespace Task45.Controllers
{
    [Route("api/")]
    [ApiController]
    public class BirthDateController : ControllerBase
    {
        [HttpGet("birthdate")]
        public ActionResult<string> CalculateAge(
            [FromQuery] string name = "anonymous", 
            [FromQuery] int? year = null, 
            [FromQuery] int? month = null, 
            [FromQuery] int? day = null)
        {
            if (year == null || month == null || day == null)
            {
                return BadRequest($"Hello {name}, I can't calculate your age without knowing your birthdate");
            }

            if (year < 0 || month < 0 || day < 0)
            {
                return BadRequest($"Hello {name}, birthdate values must be positive numbers");
            }

            if (year == 0 && month == 0 && day == 0)
            {
                return BadRequest($"Hello {name}, all birthdate values cannot be zero");
            }
            DateTime today = DateTime.Today;
            DateTime birthDate;
            
            try
            {
                birthDate = new DateTime(year.Value, month.Value, day.Value);
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
