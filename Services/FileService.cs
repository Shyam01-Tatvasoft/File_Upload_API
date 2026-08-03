using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Backend.DTOs;
using Backend.Interfaces;
using Backend.Models;
using Backend.Helpers;

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
            var savedEntity = await UploadSingleInternalAsync(dto.File, dto.Folder);
            return _mapper.Map<FileResponseDto>(savedEntity);
        }

        public async Task<List<FileResponseDto>> UploadMultipleFilesAsync(FileUploadMultipleDto dto)
        {
            var results = new List<FileResponseDto>();

            foreach (var file in dto.Files)
            {
                var savedEntity = await UploadSingleInternalAsync(file, dto.Folder);
                results.Add(_mapper.Map<FileResponseDto>(savedEntity));
            }

            return results;
        }

        private async Task<FileEntity> UploadSingleInternalAsync(Microsoft.AspNetCore.Http.IFormFile file, string? folderOverride)
        {
            var folder = string.IsNullOrWhiteSpace(folderOverride) ? "file-upload-app/general" : folderOverride;
            var isImage = FileTypeHelper.IsImage(file.FileName);

            await using var stream = file.OpenReadStream();

            RawUploadResult uploadResult;

            if (isImage)
            {
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                uploadResult = await _cloudinary.UploadAsync(imageParams);
            }
            else
            {
                var rawParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                uploadResult = await _cloudinary.UploadAsync(rawParams);
            }

            if (uploadResult.Error != null)
            {
                throw new ApplicationException($"Cloudinary upload failed for {file.FileName}: {uploadResult.Error.Message}");
            }

            var entity = new FileEntity
            {
                FileName = uploadResult.PublicId,
                OriginalFileName = file.FileName,
                PublicId = uploadResult.PublicId,
                SecureUrl = uploadResult.SecureUrl.ToString(),
                FileType = uploadResult.ResourceType,
                Extension = Path.GetExtension(file.FileName),
                FileSize = uploadResult.Bytes,
                Width = isImage ? (uploadResult as ImageUploadResult)?.Width : null,
                Height = isImage ? (uploadResult as ImageUploadResult)?.Height : null,
                Folder = folder,
                CreatedAt = DateTime.UtcNow
            };

            return await _fileRepository.AddAsync(entity);
        }
    }
}