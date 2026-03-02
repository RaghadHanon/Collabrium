using API.Contracts.Sprints;
using FluentValidation;

namespace API.Validators.Sprints;

public sealed class StartSprintRequestValidator : AbstractValidator<StartSprintRequest>
{
    public StartSprintRequestValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("EndDate is required.")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be >= StartDate.");
    }
}