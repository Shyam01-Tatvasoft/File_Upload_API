using Backend.Models;
using Backend.DTOs;

namespace Backend.Interfaces
{
    public interface IFileRepository
    {
        Task<FileEntity> AddAsync(FileEntity entity);
        Task<FileEntity?> GetByIdAsync(int id);
        Task<(List<FileEntity> Items, int TotalCount)> GetAllAsync(FileQueryParamsDto queryParams);
        Task<bool> DeleteAsync(FileEntity entity);
    }
}