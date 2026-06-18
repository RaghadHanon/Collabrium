using API.Contracts.Sprints;
using FluentValidation;

namespace API.Validators.Sprints;

public sealed class StartSprintRequestValidator : AbstractValidator<StartSprintRequest>
{
    public StartSprintRequestValidator()
    {
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