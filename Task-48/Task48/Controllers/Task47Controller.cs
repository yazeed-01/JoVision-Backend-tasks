using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Task47.Controllers
{
    [Route("api/file/")]
    [ApiController]
    public class FileController : ControllerBase
    {
        // base path
        private readonly string _baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        public FileController()
        {
            if (!Directory.Exists(_baseUploadPath))
            {
                Directory.CreateDirectory(_baseUploadPath);
            }
        }

        private string GetFormattedDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        [HttpPost("create")]
        public IActionResult UploadFile([FromForm] IFormFile file, [FromForm] string owner)
        {
            string ownerNamePattern = @"^[a-zA-Z0-9_\s]+$";

            if (file == null || file.Length == 0 || Path.GetExtension(file.FileName).ToLower() != ".jpg")
            {
                return BadRequest("Invalid file format. Only JPG files are allowed");
            }

            if (string.IsNullOrEmpty(owner) || string.IsNullOrWhiteSpace(owner))
            {
                return BadRequest("Owner is required");
            }
            if (!Regex.IsMatch(owner, ownerNamePattern))
            {
                return BadRequest("Invalid owner name. Only letters, numbers, spaces, underscores are allowed");
            }

            // path based on owner
            string ownerUploadPath = Path.Combine(_baseUploadPath, owner);
            if (!Directory.Exists(ownerUploadPath))
            {
                Directory.CreateDirectory(ownerUploadPath);
            }

            string filePath = Path.Combine(ownerUploadPath, file.FileName);
            string metadataPath = filePath + ".json";

            if (System.IO.File.Exists(filePath))
            {
                return BadRequest("File already exists");
            }

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var metadata = new
                {
                    Owner = owner,
                    CreatedAt = GetFormattedDate(DateTime.UtcNow),
                    LastModified = GetFormattedDate(DateTime.UtcNow)
                };

                System.IO.File.WriteAllText(metadataPath, JsonSerializer.Serialize(metadata));

                return Ok("File uploaded successfully");
            }
            catch (Exception)
            {
                return BadRequest("Internal server error while uploading file");
            }
        }

        [HttpGet("delete")]
        public IActionResult DeleteFile([FromQuery] string fileName, [FromQuery] string fileOwner)
        {
            string ownerUploadPath = Path.Combine(_baseUploadPath, fileOwner);
            string filePath = Path.Combine(ownerUploadPath, fileName);
            string metadataPath = filePath + ".json";

            if (!System.IO.File.Exists(filePath) || !System.IO.File.Exists(metadataPath))
            {
                return BadRequest("File not found");
            }

            try
            {
                var metadataJson = System.IO.File.ReadAllText(metadataPath);
                using JsonDocument doc = JsonDocument.Parse(metadataJson);
                JsonElement metadata = doc.RootElement;

                if (metadata.GetProperty("Owner").GetString() != fileOwner)
                {
                    return Forbid();
                }

                System.IO.File.Delete(filePath);
                System.IO.File.Delete(metadataPath);

                return Ok("File deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Internal server error: {ex.Message}");
            }
        }
    }
}
