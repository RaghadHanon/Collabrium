using Application.Features.Announcements.DTOs;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Announcements;

public sealed class AnnouncementAudienceInputDtoValidator
    : AbstractValidator<AnnouncementAudienceInputDto>
{
    public AnnouncementAudienceInputDtoValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum();

        When(x => x.Type == AnnouncementAudienceType.Everyone, () =>
        {
            RuleFor(x => x.TeamId)
                .Must(string.IsNullOrWhiteSpace)
                .WithMessage("TeamId must be empty when audience type is Everyone.");

            RuleFor(x => x.RoleName)
                .Must(x => !x.HasValue)
                .WithMessage("RoleName must be empty when audience type is Everyone.");
        });

        When(x => x.Type == AnnouncementAudienceType.Team, () =>
        {
            RuleFor(x => x.TeamId)
                .NotEmpty()
                .WithMessage("TeamId is required when audience type is Team.");

            RuleFor(x => x.RoleName)
                .Must(x => !x.HasValue)
                .WithMessage("RoleName must be empty when audience type is Team.");
        });

        When(x => x.Type == AnnouncementAudienceType.Role, () =>
        {
            RuleFor(x => x.RoleName)
                .NotNull()
                .WithMessage("RoleName is required when audience type is Role.");

            RuleFor(x => x.TeamId)
                .Must(string.IsNullOrWhiteSpace)
                .WithMessage("TeamId must be empty when audience type is Role.");
        });
    }
}