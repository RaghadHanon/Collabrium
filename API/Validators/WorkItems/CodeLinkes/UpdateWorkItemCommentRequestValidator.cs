using API.Contracts.Users;
using API.Contracts.WorkItems.Comments;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems.Comments;

public class UpdateWorkItemCodeLinkRequestValidator : AbstractValidator<UpdateWorkItemCodeLinkRequest>
{
    public UpdateWorkItemCodeLinkRequestValidator()
    {
        RuleFor(x => x.Url)
            .Must(BeValidUrl)
            .When(x => x.Url != null)
            .WithMessage("Url must be a valid.");

        RuleFor(x => x.LinkType)
            .IsInEnum()
            .When(x => x.LinkType != null)
            .WithMessage("Invalid LinkType value.");

        RuleFor(x => x)
            .Must(HasAnyField)
            .WithMessage("At least one field must be provided to update the code link.");
        }

    private static bool HasAnyField(UpdateWorkItemCodeLinkRequest r)
    {
        return r.Url != null || r.LinkType != null || r.DisplayName != null;
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}