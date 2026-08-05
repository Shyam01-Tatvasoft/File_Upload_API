namespace Backend.DTOs
{
    public class MultiUploadResultDto
    {
        public List<FileResponseDto> SucceededFiles { get; set; } = new();
        public List<FileUploadErrorDto> FailedFiles { get; set; } = new();
    }
}