using API.Contracts.Sprints;
using FluentValidation;

namespace API.Validators.Sprints;

public sealed class UpdateSprintRequestValidator : AbstractValidator<UpdateSprintRequest>
{
    public UpdateSprintRequestValidator()
    {
        When(x => x.PlannedStartDate.HasValue, () =>
        {
            RuleFor(x => x.PlannedStartDate!.Value)
                .NotEmpty()
                .WithMessage("PlannedStartDate cannot be empty.");
        });

        When(x => x.PlannedStartDate.HasValue && x.PlannedEndDate.HasValue, () =>
        {
            RuleFor(x => x.PlannedEndDate!.Value)
                .GreaterThanOrEqualTo(x => x.PlannedStartDate!.Value)
                .WithMessage("PlannedEndDate must be >= PlannedStartDate.");
        });


        RuleFor(x => x.Name)
            .MaximumLength(250)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Goal)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Goal))
            .WithMessage("Goal cannot exceed 500 characters.");

        RuleFor(x => x)
            .Must(HasAnyField)
            .WithMessage("At least one field must be provided to update the sprint.");
    }

    private static bool HasAnyField(UpdateSprintRequest r)
    {
        return r.PlannedStartDate.HasValue
                || r.Name != null
                || r.Goal != null
                || r.PlannedEndDate.HasValue;
    }
}
