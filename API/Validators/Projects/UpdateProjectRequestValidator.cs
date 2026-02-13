using API.Contracts.Projects;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Projects;

public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
            .When(x => !(x.Description is null));

        RuleFor(x => x.Status)
            .Must(status => Enum.IsDefined(typeof(ProjectStatus), status))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be a valid ProjectStatus value.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be greater than or equal to StartDate.");

        RuleFor(x => x)
            .Must(HasAnyField)
            .WithMessage("At least one field must be provided to update the project.");
    }

    private static bool HasAnyField(UpdateProjectRequest r)
    {
        return !string.IsNullOrWhiteSpace(r.Name)
               || !(r.Description is null)
               || !string.IsNullOrWhiteSpace(r.Status)
               || r.StartDate.HasValue
               || r.EndDate.HasValue
               || r.TeamId is not null;
    }
}
