using Backend.DTOs;

namespace Backend.Interfaces
{
    public interface IFileService
    {
        Task<FileResponseDto> UploadFileAsync(FileUploadDto dto);
        Task<List<FileResponseDto>> UploadMultipleFilesAsync(FileUploadMultipleDto dto);
        Task<PagedResultDto<FileResponseDto>> GetFilesAsync(FileQueryParamsDto queryParams);
        Task<FileResponseDto?> GetByIdAsync(int id);
        Task<bool> DeleteFileAsync(int id);
        Task<FileResponseDto?> UpdateFileAsync(int id, FileUpdateDto dto);
    }
}