using API.Contracts.Projects;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Projects;

public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Code cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
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
        return !(r.Name is null)
               || !(r.Description is null)
               || r.Status.HasValue
               || r.StartDate.HasValue
               || r.EndDate.HasValue;
    }
}
