using API.Contracts.Projects;
using FluentValidation;

namespace API.Validators.Projects;

public sealed class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(100).WithMessage("Code cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.TeamId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.TeamId is not null)
            .WithMessage("TeamId cannot be empty.");

        RuleFor(x => x.Key)
            .NotEmpty()
            .Matches("^[A-Z]{2,10}$")
            .WithMessage("Project key must be 2-10 uppercase letters only.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("PlannedEndDate must be greater than or equal to PlannedStartDate.");
    }
}
