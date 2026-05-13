using API.Contracts.Tenants;
using FluentValidation;

namespace API.Validators.Courses;



public class UploadImageRequestValidator : AbstractValidator<UploadImageRequest>
{
    public UploadImageRequestValidator()
    {
        RuleFor(x => x.File)
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
    };

        return allowed.Contains(file?.ContentType);
    }
}