using API.Contracts.Sprints;
using FluentValidation;

namespace API.Validators.Sprints;

public sealed class StartSprintRequestValidator : AbstractValidator<StartSprintRequest>
{
    public StartSprintRequestValidator()
    {
        RuleFor(x => x.PlannedStartDate)
            .NotEmpty()
            .WithMessage("PlannedStartDate is required.");

        RuleFor(x => x.PlannedEndDate)
            .NotEmpty().WithMessage("PlannedEndDate is required.")
            .GreaterThanOrEqualTo(x => x.PlannedStartDate)
            .WithMessage("PlannedEndDate must be >= PlannedStartDate.");

        RuleFor(x => x.Name)
            .MaximumLength(250)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Goal)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Goal))
            .WithMessage("Goal cannot exceed 500 characters.");
    }
}