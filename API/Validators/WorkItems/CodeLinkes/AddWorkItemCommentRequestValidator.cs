using API.Contracts.WorkItems.CodeLinkes;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems.CodeLinkes;

public class AddWorkItemCodeLinkRequestValidator : AbstractValidator<AddWorkItemCodeLinkRequest>
{
    public AddWorkItemCodeLinkRequestValidator()
    {
        RuleFor(x => x.Url)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Url can't be empty or null")
            .Must(BeValidUrl)
            .WithMessage("Url must be a valid.");

        RuleFor(x => x.LinkType)
            .Must(v => Enum.TryParse<CodeLinkType>(v, true, out _))
            .WithMessage("Invalid LinkType value.");

    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
