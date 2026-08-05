using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Backend.DTOs;
using Backend.Interfaces;
using Backend.Models;
using Backend.Helpers;
using CloudinaryDotNet.Actions;


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

        public async Task<MultiUploadResultDto> UploadMultipleFilesAsync(FileUploadMultipleDto dto)
        {
            var succeeded = new List<FileResponseDto>();
            var failed = new List<FileUploadErrorDto>();

            foreach (var file in dto.Files)
            {
                try
                {
                    var savedEntity = await UploadSingleInternalAsync(file, dto.Folder);
                    succeeded.Add(_mapper.Map<FileResponseDto>(savedEntity));
                }
                catch (Exception ex)
                {
                    failed.Add(new FileUploadErrorDto
                    {
                        FileName = file.FileName,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return new MultiUploadResultDto
            {
                SucceededFiles = succeeded,
                FailedFiles = failed
            };
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

        public async Task<PagedResultDto<FileResponseDto>> GetFilesAsync(FileQueryParamsDto queryParams)
        {
            var (items, totalCount) = await _fileRepository.GetAllAsync(queryParams);

            return new PagedResultDto<FileResponseDto>
            {
                Items = _mapper.Map<List<FileResponseDto>>(items),
                TotalCount = totalCount,
                Page = queryParams.Page < 1 ? 1 : queryParams.Page,
                PageSize = queryParams.PageSize is < 1 or > 100 ? 10 : queryParams.PageSize
            };
        }

        public async Task<FileResponseDto?> GetByIdAsync(int id)
        {
            var entity = await _fileRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<FileResponseDto>(entity);
        }

        public async Task<bool> DeleteFileAsync(int id)
        {
            var entity = await _fileRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return false; // Controller will translate this into 404
            }

            var resourceType = entity.FileType == "image" ? ResourceType.Image : ResourceType.Raw;

            var deletionParams = new DeletionParams(entity.PublicId)
            {
                ResourceType = resourceType
            };

            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.Result != "ok" && deletionResult.Result != "not found")
            {
                throw new ApplicationException(
                    $"Failed to delete file from Cloudinary: {deletionResult.Result}");
            }

            return await _fileRepository.DeleteAsync(entity);
        }



        public async Task<FileResponseDto?> UpdateFileAsync(int id, FileUpdateDto dto)
        {
            var entity = await _fileRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return null; // Controller translates this into 404
            }

            // Case 1: A new file was provided — replace the content on Cloudinary
            if (dto.NewFile != null)
            {
                // Step A: Delete the old file from Cloudinary (reusing Module 9's approach)
                var oldResourceType = entity.FileType == "image" ? ResourceType.Image : ResourceType.Raw;
                var deletionParams = new DeletionParams(entity.PublicId) { ResourceType = oldResourceType };
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                if (deletionResult.Result != "ok" && deletionResult.Result != "not found")
                {
                    throw new ApplicationException(
                        $"Failed to delete old file from Cloudinary during update: {deletionResult.Result}");
                }

                // Step B: Upload the new file (reusing Module 7's approach)
                var isImage = FileTypeHelper.IsImage(dto.NewFile.FileName);
                await using var stream = dto.NewFile.OpenReadStream();

                RawUploadResult uploadResult;

                if (isImage)
                {
                    var imageParams = new ImageUploadParams
                    {
                        File = new FileDescription(dto.NewFile.FileName, stream),
                        Folder = entity.Folder,
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
                        File = new FileDescription(dto.NewFile.FileName, stream),
                        Folder = entity.Folder,
                        UseFilename = true,
                        UniqueFilename = true,
                        Overwrite = false
                    };
                    uploadResult = await _cloudinary.UploadAsync(rawParams);
                }

                if (uploadResult.Error != null)
                {
                    throw new ApplicationException(
                        $"Failed to upload new file during update: {uploadResult.Error.Message}");
                }

                // Step C: Update the entity's file-related fields with the new upload's data
                entity.PublicId = uploadResult.PublicId;
                entity.FileName = uploadResult.PublicId;
                entity.SecureUrl = uploadResult.SecureUrl.ToString();
                entity.FileType = uploadResult.ResourceType;
                entity.Extension = Path.GetExtension(dto.NewFile.FileName);
                entity.FileSize = uploadResult.Bytes;
                entity.Width = isImage ? (uploadResult as ImageUploadResult)?.Width : null;
                entity.Height = isImage ? (uploadResult as ImageUploadResult)?.Height : null;

                // If the client didn't explicitly provide a new display name,
                // default to the new file's own name
                if (string.IsNullOrWhiteSpace(dto.OriginalFileName))
                {
                    entity.OriginalFileName = dto.NewFile.FileName;
                }
            }

            // Case 2: Rename only (applies whether or not a new file was also uploaded)
            if (!string.IsNullOrWhiteSpace(dto.OriginalFileName))
            {
                entity.OriginalFileName = dto.OriginalFileName;
            }

            entity.UpdatedAt = DateTime.UtcNow;

            var updatedEntity = await _fileRepository.UpdateAsync(entity);
            return _mapper.Map<FileResponseDto>(updatedEntity);
        }
    }
}