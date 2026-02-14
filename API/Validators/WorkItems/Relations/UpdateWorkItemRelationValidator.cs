using API.Contracts.WorkItems.Relations;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems.Relations;

public class UpdateWorkItemRelationValidator : AbstractValidator<UpdateWorkItemRelationRequest>
{
    public UpdateWorkItemRelationValidator()
    {
        RuleFor(x => x.NewTargetWorkItemId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("NewTargetWorkItemId can't be empty or null");

        RuleFor(x => x.NewRelationType)
            .Must(v => Enum.TryParse<WorkItemRelationType>(v, true, out _))
            .WithMessage("RelationType must be a valid RelationType value.");
    }
}