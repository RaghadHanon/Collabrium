using API.Contracts.Projects;
using API.Contracts.Tenants;
using FluentValidation;

namespace API.Validators.Tenants;

public sealed class UpdateTenantRequestValidator : AbstractValidator<UpdateTenantRequest>
{
    public UpdateTenantRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Domain)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Domain))
            .WithMessage("Domain cannot exceed 100 characters.")
            .Matches(@"^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Domain))
            .WithMessage("Domain must be a valid domain format (e.g., example.com).");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Country))
            .WithMessage("Country cannot exceed 100 characters.");

        RuleFor(x => x.WebsiteURL)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.WebsiteURL))
            .WithMessage("WebsiteURL must be a valid URL.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("A valid email is required.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{9,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be valid and contain only digits (optionally starting with +).");

        RuleFor(x => x.LinkedIn)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LinkedIn))
            .WithMessage("LinkedIn must be a valid URL.");

        RuleFor(x => x.Facebook)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.Facebook))
            .WithMessage("Facebook must be a valid URL.");

        RuleFor(x => x.Instagram)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.Instagram))
            .WithMessage("Instagram must be a valid URL.");

        RuleFor(x => x.YouTube)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.YouTube))
            .WithMessage("YouTube must be a valid URL.");

        RuleFor(x => x.File)
            .Must(BeValidImageFile)
            .When(x => x.File is not null)
            .WithMessage("File must be an image (jpg, jpeg, png, webp).");

        RuleFor(x => x)
           .Must(HasAnyField)
           .WithMessage("At least one field must be provided to update the project.");
    }

    private static bool HasAnyField(UpdateTenantRequest r)
        => !string.IsNullOrWhiteSpace(r.Name)
          || !string.IsNullOrWhiteSpace(r.Description)
          || !string.IsNullOrWhiteSpace(r.Domain)
          || !string.IsNullOrWhiteSpace(r.Country)
          || !string.IsNullOrWhiteSpace(r.WebsiteURL)
          || !string.IsNullOrWhiteSpace(r.Email)
          || !string.IsNullOrWhiteSpace(r.PhoneNumber)
          || !string.IsNullOrWhiteSpace(r.LinkedIn)
          || !string.IsNullOrWhiteSpace(r.Facebook)
          || !string.IsNullOrWhiteSpace(r.Instagram)
          || !string.IsNullOrWhiteSpace(r.YouTube)
          || r.File is not null;

    private static bool BeValidUrl(string? url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static bool BeValidImageFile(IFormFile? file)
    {
        if (file is null) return true;

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        return allowed.Contains(file.ContentType);
    }
}