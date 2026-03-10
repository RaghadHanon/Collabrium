using API.Contracts.Tenants;
using FluentValidation;

namespace API.Validators.Tenants;

public sealed class UpdateTenantRequestValidator : AbstractValidator<UpdateTenantRequest>
{
    public UpdateTenantRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name) && x.Name != "_NULL")
            .WithMessage("Code cannot exceed 100 characters.");

        RuleFor(x => x.Domain)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Domain) && x.Domain != "_NULL")
            .WithMessage("Domain cannot exceed 100 characters.")
            .Matches(@"^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)+$")
            .When(x => !string.IsNullOrEmpty(x.Domain) && x.Domain != "_NULL")
            .WithMessage("Domain must be a valid domain format (e.g., example.com).");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description) && x.Description != "_NULL")
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Country) && x.Country != "_NULL")
            .WithMessage("Country cannot exceed 100 characters.");

        RuleFor(x => x.WebsiteURL)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.WebsiteURL) && x.WebsiteURL != "_NULL")
            .WithMessage("WebsiteURL must be a valid URL.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email) && x.Email != "_NULL")
            .WithMessage("A valid email is required.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{9,15}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber != "_NULL")
            .WithMessage("Phone number must be valid and contain only digits (optionally starting with +).");

        RuleFor(x => x.LinkedIn)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.LinkedIn) && x.LinkedIn != "_NULL")
            .WithMessage("LinkedIn must be a valid URL.");

        RuleFor(x => x.Facebook)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.Facebook) && x.Facebook != "_NULL")
            .WithMessage("Facebook must be a valid URL.");

        RuleFor(x => x.Instagram)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.Instagram) && x.Instagram != "_NULL")
            .WithMessage("Instagram must be a valid URL.");

        RuleFor(x => x.YouTube)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.YouTube) && x.YouTube != "_NULL")
            .WithMessage("YouTube must be a valid URL.");

        RuleFor(x => x.File)
            .Must(BeValidImageFile)
            .When(x => x.File is not null)
            .WithMessage("File must be an image (jpg, jpeg, png, webp).");

        RuleFor(x => x)
           .Must(HasAnyField)
           .WithMessage("At least one field must be provided to update the Tenant.");
    }

    private static bool HasAnyField(UpdateTenantRequest r)
    {
        return !(r.Name is null)
              || !(r.Description is null)
              || !(r.Domain is null)
              || !(r.Country is null)
              || !(r.WebsiteURL is null)
              || !(r.Email is null)
              || !(r.PhoneNumber is null)
              || !(r.LinkedIn is null)
              || !(r.Facebook is null)
              || !(r.Instagram is null)
              || !(r.YouTube is null)
              || r.File is not null;
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

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