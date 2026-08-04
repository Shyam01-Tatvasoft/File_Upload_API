using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Backend.DTOs;

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

        public async Task<(List<FileEntity> Items, int TotalCount)> GetAllAsync(FileQueryParamsDto queryParams)
        {
            var query = _context.Files.AsQueryable();

            // 1. Search filter
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.Trim().ToLower();
                query = query.Where(f =>
                    f.OriginalFileName.ToLower().Contains(search) ||
                    f.Folder.ToLower().Contains(search));
            }

            // 2. Get total count BEFORE pagination (but after filtering)
            var totalCount = await query.CountAsync();

            // 3. Sorting
            query = queryParams.SortBy?.ToLower() switch
            {
                "originalfilename" => queryParams.SortDescending
                    ? query.OrderByDescending(f => f.OriginalFileName)
                    : query.OrderBy(f => f.OriginalFileName),
                "filesize" => queryParams.SortDescending
                    ? query.OrderByDescending(f => f.FileSize)
                    : query.OrderBy(f => f.FileSize),
                _ => queryParams.SortDescending
                    ? query.OrderByDescending(f => f.CreatedAt)
                    : query.OrderBy(f => f.CreatedAt)
            };

            // 4. Pagination
            var page = queryParams.Page < 1 ? 1 : queryParams.Page;
            var pageSize = queryParams.PageSize is < 1 or > 100 ? 10 : queryParams.PageSize;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}