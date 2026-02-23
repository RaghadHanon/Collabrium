using API.Contracts.Projects;
using FluentValidation;

namespace API.Validators.Projects;

public sealed class AssignProjectToTeamRequestValidator : AbstractValidator<AssignProjectToTeamRequest>
{
    public AssignProjectToTeamRequestValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("TeamId is required.");
    }
}
