using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Backend.DTOs;
using Backend.Interfaces;
using Backend.Models;

namespace Backend.Services
{
    public class FileService : IFileService
    {
        private readonly Cloudinary _cloudinary;
        private readonly IFileRepository _fileRepository;
        private readonly IMapper _mapper;

        public FileService(Cloudinary cloudinary, IFileRepository fileRepository, IMapper mapper)
        {
            _cloudinary = cloudinary;
            _fileRepository = fileRepository;
            _mapper = mapper;
        }

        public async Task<FileResponseDto> UploadFileAsync(FileUploadDto dto)
        {
            var folder = string.IsNullOrWhiteSpace(dto.Folder) ? "file-upload-app/general" : dto.Folder;

            await using var stream = dto.File.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(dto.File.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new ApplicationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            var entity = new FileEntity
            {
                FileName = uploadResult.PublicId,
                OriginalFileName = dto.File.FileName,
                PublicId = uploadResult.PublicId,
                SecureUrl = uploadResult.SecureUrl.ToString(),
                FileType = uploadResult.ResourceType,
                Extension = Path.GetExtension(dto.File.FileName),
                FileSize = uploadResult.Bytes,
                Width = uploadResult.Width,
                Height = uploadResult.Height,
                Folder = folder,
                CreatedAt = DateTime.UtcNow
            };

            var savedEntity = await _fileRepository.AddAsync(entity);

            return _mapper.Map<FileResponseDto>(savedEntity);
        }
    }
}