using FluentValidation;
using Backend.DTOs;

namespace Backend.Validators
{
    public class FileUploadDtoValidator : AbstractValidator<FileUploadDto>
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public FileUploadDtoValidator()
        {
            RuleFor(x => x.File)
                .NotNull().WithMessage("A file is required.")
                .Must(HaveAFileName).WithMessage("The uploaded file must have a valid filename.")
                .Must(BeAValidSize).WithMessage($"File size must not exceed {MaxFileSize / (1024 * 1024)} MB.")
                .Must(BeAValidExtension).WithMessage("Only image files (.jpg, .jpeg, .png, .gif, .webp) and PDFs are allowed.");
        }

        private bool HaveAFileName(Microsoft.AspNetCore.Http.IFormFile file)
        {
            return file != null && !string.IsNullOrWhiteSpace(file.FileName);
        }

        private bool BeAValidSize(Microsoft.AspNetCore.Http.IFormFile file)
        {
            return file != null && file.Length > 0 && file.Length <= MaxFileSize;
        }

        private bool BeAValidExtension(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null) return false;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }
    }
}