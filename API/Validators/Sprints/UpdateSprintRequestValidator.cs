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
                .NotEmpty().WithMessage("StartDate cannot be empty.");
        });

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate!.Value)
                .GreaterThanOrEqualTo(x => x.StartDate!.Value)
                .WithMessage("EndDate must be >= StartDate.");
        });

        RuleFor(x => x)
            .Must(HasAnyField)
            .WithMessage("At least one field must be provided to update the sprint.");
    }

    private static bool HasAnyField(UpdateSprintRequest r)
    {
        return r.StartDate.HasValue
               || r.EndDate.HasValue;
    }
}
