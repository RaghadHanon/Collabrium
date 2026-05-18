using API.Contracts.Announcements;
using FluentValidation;

namespace API.Validators.Announcements;

public sealed class UpdateAnnouncementRequestValidator
    : AbstractValidator<UpdateAnnouncementRequest>
{
    public UpdateAnnouncementRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .MaximumLength(4000);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority.HasValue);

        RuleFor(x => x)
            .Must(x =>
            {
                if (x.ClearExpiresAt)
                    return true;

                if (!x.ExpiresAt.HasValue || !x.PublishAt.HasValue)
                    return true;

                return x.ExpiresAt > x.PublishAt;
            })
            .WithMessage("ExpiresAt must be after PublishAt.");

        RuleFor(x => x.Audiences)
            .Must(x => x is null || x.Count > 0)
            .WithMessage("Announcement must have at least one audience when audiences is provided.");

        RuleForEach(x => x.Audiences!)
            .SetValidator(new AnnouncementAudienceInputDtoValidator())
            .When(x => x.Audiences is not null);
    }
}