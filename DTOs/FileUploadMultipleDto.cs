using Microsoft.AspNetCore.Http;

namespace Backend.DTOs
{
    public class FileUploadMultipleDto
    {
        public List<IFormFile> Files { get; set; } = new();
        public string? Folder { get; set; }
    }
}