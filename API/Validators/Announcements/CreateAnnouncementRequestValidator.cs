using API.Contracts.Announcements;
using FluentValidation;

namespace API.Validators.Announcements;

public sealed class CreateAnnouncementRequestValidator
    : AbstractValidator<CreateAnnouncementRequest>
{
    public CreateAnnouncementRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.PublishAt)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x => !x.ExpiresAt.HasValue || x.ExpiresAt > x.PublishAt)
            .WithMessage("ExpiresAt must be after PublishAt.");

        RuleFor(x => x.Audiences)
            .NotEmpty()
            .WithMessage("Announcement must have at least one audience.");

        RuleForEach(x => x.Audiences)
            .SetValidator(new AnnouncementAudienceInputDtoValidator());
    }
}