using API.Contracts.Teams;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Teams;

public sealed class CreateTeamRequestValidator : AbstractValidator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleFor(x => x.TeamLeadId)
            .NotEmpty().WithMessage("TeamLeadId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(100).WithMessage("Code cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .IsInEnum()
            .WithMessage("Department must be a valid value.");

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Capacity must be >= 1.");
    }
}
