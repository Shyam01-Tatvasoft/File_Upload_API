using Microsoft.AspNetCore.Http;

namespace Backend.DTOs
{
    public class FileUploadDto
    {
        public IFormFile File { get; set; } = null!;
        public string? Folder { get; set; }
    }
}