namespace Backend.DTOs
{
    public class FileResponseDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string SecureUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Folder { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}