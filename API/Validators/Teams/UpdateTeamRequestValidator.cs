using API.Contracts.Teams;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Teams;

public sealed class UpdateTeamRequestValidator : AbstractValidator<UpdateTeamRequest>
{
    public UpdateTeamRequestValidator()
    {
        RuleFor(x => x.TeamLeadId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.TeamLeadId is not null)
            .WithMessage("TeamLeadId cannot be empty.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Code cannot exceed 100 characters.")
            .When(x => !(x.Name is null));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !(x.Description is null));

        RuleFor(x => x.Department)
            .IsInEnum()
            .When(x => !(x.Department is null))
            .WithMessage("Department must be a valid value.");

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(1)
            .When(x => x.Capacity.HasValue)
            .WithMessage("Capacity must be >= 1 if provided.");

        RuleFor(x => x)
            .Must(HasAnyField)
            .WithMessage("At least one field must be provided to update the team.");
    }

    private static bool HasAnyField(UpdateTeamRequest r)
    {
        return !string.IsNullOrWhiteSpace(r.TeamLeadId)
               || !(r.Name is null)
               || !(r.Description is null)
               || !r.Department.HasValue
               || r.Capacity.HasValue;
    }
}
