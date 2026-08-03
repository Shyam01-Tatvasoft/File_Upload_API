using FluentValidation;
using Backend.DTOs;

namespace Backend.Validators
{
    public class FileUploadDtoValidator : AbstractValidator<FileUploadDto>
    {
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedDocumentExtensions = { ".pdf" };
        private const long MaxImageSize = 10 * 1024 * 1024;   // 10 MB
        private const long MaxDocumentSize = 20 * 1024 * 1024; // 20 MB

        public FileUploadDtoValidator()
        {
            RuleFor(x => x.File)
                .NotNull().WithMessage("A file is required.")
                .Must(HaveAllowedExtension).WithMessage("Only images (.jpg, .jpeg, .png, .gif, .webp) and PDFs are allowed.")
                .Must(BeWithinSizeLimit).WithMessage("File exceeds the allowed size limit for its type.");
        }

        private bool HaveAllowedExtension(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null) return false;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedImageExtensions.Contains(extension) || _allowedDocumentExtensions.Contains(extension);
        }

        private bool BeWithinSizeLimit(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null) return false;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (_allowedImageExtensions.Contains(extension))
                return file.Length > 0 && file.Length <= MaxImageSize;

            if (_allowedDocumentExtensions.Contains(extension))
                return file.Length > 0 && file.Length <= MaxDocumentSize;

            return false;
        }
    }
}