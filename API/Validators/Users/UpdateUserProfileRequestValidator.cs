using API.Contracts.Users;
using FluentValidation;

namespace API.Validators.Users;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {

        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.FirstName))
            .WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.LastName))
            .WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18))
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("User must be at least 18 years old.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{9,15}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number must be valid and contain only digits (optionally starting with +).");

        RuleFor(x => x.LinkedIn)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.LinkedIn))
            .WithMessage("LinkedIn must be a valid URL.");

        RuleFor(x => x.Facebook)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.Facebook))
            .WithMessage("Facebook must be a valid URL.");

        RuleFor(x => x.Instagram)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.Instagram))
            .WithMessage("Instagram must be a valid URL.");

        RuleFor(x => x.YouTube)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.YouTube))
            .WithMessage("YouTube must be a valid URL.");

        RuleFor(x => x.Github)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.Github))
            .WithMessage("Github must be a valid URL.");

        RuleFor(x => x)
           .Must(HasAnyField)
           .WithMessage("At least one field must be provided to update the user profile.");
    }

    private static bool HasAnyField(UpdateUserProfileRequest r)
    {
        return !(r.FirstName is null)
               || !(r.LastName is null)
               || r.DateOfBirth.HasValue
               || !(r.PhoneNumber is null)
               || !(r.LinkedIn is null)
               || !(r.Facebook is null)
               || !(r.Instagram is null)
               || !(r.YouTube is null)
               || !(r.Github is null);
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}