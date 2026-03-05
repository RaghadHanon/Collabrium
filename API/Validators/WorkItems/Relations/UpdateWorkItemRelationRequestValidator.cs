using API.Contracts.WorkItems.Relations;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems.Relations;

public class UpdateWorkItemRelationRequestValidator : AbstractValidator<UpdateWorkItemRelationRequest>
{
    public UpdateWorkItemRelationRequestValidator()
    {
        RuleFor(x => x.NewTargetWorkItemId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("NewTargetWorkItemId can't be empty or null");

        RuleFor(x => x.NewRelationType)
            .IsInEnum()
            .WithMessage("RelationType must be a valid RelationType value.");
    }
}