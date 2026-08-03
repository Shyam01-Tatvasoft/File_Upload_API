using Backend.DTOs;

namespace Backend.Interfaces
{
    public interface IFileService
    {
        Task<FileResponseDto> UploadFileAsync(FileUploadDto dto);
        Task<List<FileResponseDto>> UploadMultipleFilesAsync(FileUploadMultipleDto dto);
    }
}