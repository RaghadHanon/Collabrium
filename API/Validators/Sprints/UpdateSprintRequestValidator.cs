using API.Contracts.Sprints;
using FluentValidation;

namespace API.Validators.Sprints;

public sealed class UpdateSprintRequestValidator : AbstractValidator<UpdateSprintRequest>
{
    public UpdateSprintRequestValidator()
    {
        When(x => x.StartDate.HasValue, () =>
        {
            RuleFor(x => x.StartDate!.Value)
                .NotEmpty()
                .WithMessage("StartDate cannot be empty.");
        });

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate!.Value)
                .GreaterThanOrEqualTo(x => x.StartDate!.Value)
                .WithMessage("EndDate must be >= StartDate.");
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
        return r.StartDate.HasValue
                || r.Name != null
                || r.Goal != null
                || r.EndDate.HasValue;
    }
}
