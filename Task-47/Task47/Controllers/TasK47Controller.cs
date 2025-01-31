using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Task47.Controllers
{

    /*
        For CREATE
            1. User sends a POST request with a .jpg file and owner name.
            2. Validate the file format and owner name.
            3. Check if the owner’s directory exists. Create it if not.
            4. Check if the file already exists. Reject if true.
            5. Save the new .jpg file in the owner's directory.
            6. Create and write metadata with owner, creation, and modification timestamps in .json file.
            7. Respond with success or error message.

        For DELETE
            1. User sends a GET request with fileName and fileOwner query parameters.
            2. Check if the file and metadata exist.
            3. Validate if the owner matches the metadata.
            4. Delete the .jpg file and its .json metadata file.
            5. Respond with success or error message.


    */
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
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
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
