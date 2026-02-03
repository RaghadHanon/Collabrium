using API.Contracts.Users;
using FluentValidation;

namespace API.Validators.Users;

public sealed class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
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
    }

    private static bool BeValidUrl(string? url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}