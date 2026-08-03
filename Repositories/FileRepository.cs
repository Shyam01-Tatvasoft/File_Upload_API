using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Interfaces;
using Backend.Models;

namespace Backend.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly AppDbContext _context;

        public FileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FileEntity> AddAsync(FileEntity entity)
        {
            _context.Files.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<FileEntity?> GetByIdAsync(int id)
        {
            return await _context.Files.FindAsync(id);
        }
    }
}