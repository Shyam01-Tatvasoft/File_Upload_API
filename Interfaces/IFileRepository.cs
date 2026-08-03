using Backend.Models;

namespace Backend.Interfaces
{
    public interface IFileRepository
    {
        Task<FileEntity> AddAsync(FileEntity entity);
        Task<FileEntity?> GetByIdAsync(int id);
    }
}