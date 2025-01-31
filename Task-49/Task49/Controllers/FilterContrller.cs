using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Task49.Models;

namespace Task49.Controllers
{
    [Route("api/")]
    [ApiController]
    public class FilterController : ControllerBase
    {
        private readonly string _baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        [Route("filter")]
        [HttpPost]
        public IActionResult Filter([FromForm] DateTime CreationDate, [FromForm] DateTime ModificationDate, [FromForm] string Owner, [FromForm] FilterType FilterType)
        {
            if (!Enum.IsDefined(typeof(FilterType), FilterType))
            {
                return BadRequest("Invalid FilterType");
            }

            try
            {
                var allFiles = Directory.GetFiles(_baseUploadPath, "*.json", SearchOption.AllDirectories)
                    .Select(f => new
                    {
                        FileName = Path.GetFileNameWithoutExtension(f),
                        Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(System.IO.File.ReadAllText(f))
                    });

                var result = FilterType switch
                {
                    FilterType.ByModificationDate => allFiles
                        .Where(f => DateTime.Parse(f.Metadata["LastModified"]) < ModificationDate)
                        .Select(f => new { FileName = f.FileName, OwnerName = f.Metadata["Owner"] }),

                    FilterType.ByCreationDateDescending => allFiles
                        .Where(f => DateTime.Parse(f.Metadata["CreatedAt"]) > CreationDate)
                        .OrderByDescending(f => DateTime.Parse(f.Metadata["CreatedAt"]))
                        .Select(f => new { FileName = f.FileName, OwnerName = f.Metadata["Owner"] }),

                    FilterType.ByCreationDateAscending => allFiles
                        .Where(f => DateTime.Parse(f.Metadata["CreatedAt"]) > CreationDate)
                        .OrderBy(f => DateTime.Parse(f.Metadata["CreatedAt"]))
                        .Select(f => new { FileName = f.FileName, OwnerName = f.Metadata["Owner"] }),

                    FilterType.ByOwner => allFiles
                        .Where(f => f.Metadata["Owner"] == Owner)
                        .Select(f => new { FileName = f.FileName, OwnerName = f.Metadata["Owner"] }),

                    _ => throw new ArgumentException("Invalid FilterType")
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

