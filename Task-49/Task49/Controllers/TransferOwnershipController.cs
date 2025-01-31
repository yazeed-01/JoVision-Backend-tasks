using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace Task49.Controllers
{
    [Route("api/")]
    [ApiController]
    public class TransferOwnershipController : ControllerBase
    {
        private readonly string _baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        [Route("transfer")]
        [HttpGet]
        public IActionResult TransferOwnership([FromQuery(Name = "OldOwner")] string oldOwner, [FromQuery(Name = "NewOwner")] string newOwner)
        {
            if (string.IsNullOrWhiteSpace(oldOwner) || string.IsNullOrWhiteSpace(newOwner))
            {
                return BadRequest(new { error = "OldOwner and NewOwner are required" });
            }

            if (!Directory.Exists(_baseUploadPath))
            {
                return BadRequest("Uploads directory not found");
            }

            var updatedFolders = new List<object>();

            foreach (var folder in Directory.GetDirectories(_baseUploadPath))
            {
                var jsonFiles = Directory.GetFiles(folder, "*.json");
                bool shouldRenameFolder = false;

                foreach (var file in jsonFiles)
                {
                    var jsonContent = System.IO.File.ReadAllText(file);
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);

                    if (metadata != null && metadata.TryGetValue("Owner", out var owner) && owner?.ToString() == oldOwner)
                    {
                        metadata["Owner"] = newOwner;
                        metadata["LastModified"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                        var updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                        System.IO.File.WriteAllText(file, updatedJson);

                        shouldRenameFolder = true;
                    }
                }

                if (shouldRenameFolder)
                {
                    string newFolderPath = Path.Combine(_baseUploadPath, newOwner);

                    if (!Directory.Exists(newFolderPath))
                    {
                        Directory.Move(folder, newFolderPath);
                        updatedFolders.Add(new { OldFolderName = Path.GetFileName(folder), NewFolderName = newOwner });
                    }
                    else
                    {
                        // if folder already exists, move contents instead
                        foreach (var file in Directory.GetFiles(folder))
                        {
                            var fileName = Path.GetFileName(file);
                            var destinationPath = Path.Combine(newFolderPath, fileName);
                            if (System.IO.File.Exists(destinationPath))
                            {
                                System.IO.File.Delete(destinationPath);
                            }
                            System.IO.File.Move(file, destinationPath);
                        }

                        Directory.Delete(folder, true);
                    }
                }
            }

            return Ok(updatedFolders);
        }
    }
}
