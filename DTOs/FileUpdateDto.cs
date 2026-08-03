using Microsoft.AspNetCore.Http;

namespace Backend.DTOs
{
    public class FileUpdateDto
    {
        public IFormFile? NewFile { get; set; }
        public string? OriginalFileName { get; set; }
    }
}