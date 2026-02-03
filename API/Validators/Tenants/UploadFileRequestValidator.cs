using API.Contracts.Tenants;
using FluentValidation;

namespace API.Validators.Tenants;

public sealed class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
{
    public UploadFileRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required.")
            .Must(f => f is not null && f.Length > 0)
            .WithMessage("File cannot be empty.")
            .Must(BeAllowedFile)
            .WithMessage("File type is not allowed.");
    }

    private static bool BeAllowedFile(IFormFile file)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp",
            "application/pdf"
        };

        return allowed.Contains(file.ContentType);
    }
}
