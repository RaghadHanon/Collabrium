using API.Contracts.WorkItems.Relations;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems.Relations;

public class AddWorkItemRelationRequestValidator : AbstractValidator<AddWorkItemRelationRequest>
{
    public AddWorkItemRelationRequestValidator()
    {
        RuleFor(x => x.TargetWorkItemId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("NewTargetWorkItemId can't be empty or null");

        RuleFor(x => x.RelationType)
            .Must(v => Enum.TryParse<WorkItemRelationType>(v, true, out _))
            .WithMessage("RelationType must be a valid RelationType value.");
    }
}
