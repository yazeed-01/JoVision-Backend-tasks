using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Task48.Controllers
{
    /*

        For UPDATE: 
        1. User sends a POST request with a .jpg file and an owner name.
        2. Validate the file format and owner name.
        3. Check if the owner’s directory exists. Create it if not.
        4. Delete previous .jpg image and .json metadata file (if they exist).
        5. Save the new .jpg file in the owner’s directory.
        6. Read or create metadata for the new file.
        7. Update the metadata with the current timestamp.
        8. Save the updated metadata as a .json file.
        9. Respond with success or error status.

        For RETRIEVE:
        1. User sends a GET request with fileName and fileOwner query parameters.
        2. Check if the image file and metadata .json file exist.
        3. Validate the owner in the metadata file.
        4. Read the image file and return it.
        5. Respond with the file, or return error if not found or access is forbidden.

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

        private string GetFormattedDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        [HttpPost("update")]
        public IActionResult UpdateFile([FromForm] IFormFile file, [FromForm] string owner)
        {
            string ownerNamePattern = @"^[a-zA-Z0-9_\s]+$";
            if (file == null || file.Length == 0 || Path.GetExtension(file.FileName).ToLower() != ".jpg")
            {
                return BadRequest("Invalid file format. Only JPG files are allowed");
            }
            if (string.IsNullOrWhiteSpace(owner))
            {
                return BadRequest("Owner is required");
            }
            if (!Regex.IsMatch(owner, ownerNamePattern))
            {
                return BadRequest("Invalid owner name. Only letters, numbers, spaces, underscores are allowed");
            }

            string ownerUploadPath = Path.Combine(_baseUploadPath, owner);

            try
            {
                // check if directory exists
                Directory.CreateDirectory(ownerUploadPath);

                // delete previous image file
                var previousImageFile = Directory.GetFiles(ownerUploadPath, "*.jpg").FirstOrDefault();
                if (previousImageFile != null)
                {
                    System.IO.File.Delete(previousImageFile);
                }

                // get previous JSON file
                var previousJsonFile = Directory.GetFiles(ownerUploadPath, "*.json").FirstOrDefault();
                
                // new file name and paths
                string newFileName = file.FileName;
                string newFilePath = Path.Combine(ownerUploadPath, newFileName);
                string newMetadataPath = Path.ChangeExtension(newFilePath, ".json");

                // save new image file
                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // update metadata
                Dictionary<string, string> metadata;
                if (previousJsonFile != null)
                {
                    // read existing metadata
                    metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(System.IO.File.ReadAllText(previousJsonFile));
                    
                    // delete old JSON file
                    System.IO.File.Delete(previousJsonFile);
                }
                else
                {
                    // create new metadata if no previous file exists
                    metadata = new Dictionary<string, string>
                    {
                        ["Owner"] = owner,
                        ["CreatedAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                }

                // update LastModified
                metadata["LastModified"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                // write updated metadata to new JSON file
                System.IO.File.WriteAllText(newMetadataPath, JsonSerializer.Serialize(metadata));

                return Ok("File updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("retrieve")]
        public IActionResult RetrieveFile([FromQuery] string fileName, [FromQuery] string fileOwner)
        {
            string ownerUploadPath = Path.Combine(_baseUploadPath, fileOwner);
            string filePath = Path.Combine(ownerUploadPath, fileName);
            string metadataPath = filePath + ".json";

            if (!System.IO.File.Exists(filePath) || !System.IO.File.Exists(metadataPath))
            {
                return NotFound("File not found");
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

                // read and return it
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "image/jpeg");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
