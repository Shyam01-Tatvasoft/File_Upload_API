namespace Backend.DTOs
{
    public class FileUploadErrorDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}