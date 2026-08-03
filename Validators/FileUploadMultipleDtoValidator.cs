using FluentValidation;
using Backend.DTOs;

namespace Backend.Validators
{
    public class FileUploadMultipleDtoValidator : AbstractValidator<FileUploadMultipleDto>
    {
        private const int MaxFilesPerRequest = 10;

        public FileUploadMultipleDtoValidator()
        {
            RuleFor(x => x.Files)
                .NotEmpty().WithMessage("At least one file is required.")
                .Must(files => files.Count <= MaxFilesPerRequest)
                    .WithMessage($"You can upload a maximum of {MaxFilesPerRequest} files at once.");

            RuleForEach(x => x.Files)
                .SetValidator(new SingleFileValidator());
        }
    }

    // A reusable validator for a single IFormFile, used inside RuleForEach above
    public class SingleFileValidator : AbstractValidator<Microsoft.AspNetCore.Http.IFormFile>
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf" };
        private const long MaxFileSize = 20 * 1024 * 1024;

        public SingleFileValidator()
        {
            RuleFor(f => f)
                .Must(f => f.Length > 0 && f.Length <= MaxFileSize)
                    .WithMessage("Each file must be non-empty and under 20 MB.")
                .Must(f => _allowedExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
                    .WithMessage("Each file must be a supported image or PDF type.");
        }
    }
}