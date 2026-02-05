using API.Contracts.Users;
using FluentValidation;

namespace API.Validators.Users;

public sealed class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {

        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName))
            .WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName))
            .WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18))
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("User must be at least 18 years old.");

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

        RuleFor(x => x.Github)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.Github))
            .WithMessage("Github must be a valid URL.");

        RuleFor(x => x)
           .Must(HasAnyField)
           .WithMessage("At least one field must be provided to update the project.");
    }

    private static bool HasAnyField(UpdateUserProfileRequest r)
        => !string.IsNullOrWhiteSpace(r.FirstName)
           || !string.IsNullOrWhiteSpace(r.LastName)
           || r.DateOfBirth.HasValue
           || !string.IsNullOrWhiteSpace(r.PhoneNumber)
           || !string.IsNullOrWhiteSpace(r.LinkedIn)
           || !string.IsNullOrWhiteSpace(r.Facebook)
           || !string.IsNullOrWhiteSpace(r.Instagram)
           || !string.IsNullOrWhiteSpace(r.YouTube)
           || !string.IsNullOrWhiteSpace(r.Github);

    private static bool BeValidUrl(string? url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}