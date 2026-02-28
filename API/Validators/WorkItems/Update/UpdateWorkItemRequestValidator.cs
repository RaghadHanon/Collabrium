using API.Contracts.WorkItems.Update;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems.Update;

public sealed class UpdateWorkItemRequestValidator : AbstractValidator<UpdateWorkItemRequest>
{
    public UpdateWorkItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
            .When(x => !(x.Title is null));

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.")
            .When(x => !(x.Description is null));

        RuleFor(x => x.AcceptanceCriteria)
            .MaximumLength(4000).WithMessage("AcceptanceCriteria cannot exceed 4000 characters.")
            .When(x => !(x.AcceptanceCriteria is null));

        RuleFor(x => x.EffortPoints)
            .GreaterThanOrEqualTo(0).WithMessage("EffortPoints must be greater than or equal to 0.")
            .When(x => x.EffortPoints.HasValue);

        RuleFor(x => x.Priority)
            .Must(p => Enum.TryParse<WorkItemPriority>(p, true, out _))
            .WithMessage("Priority must be a valid WorkItemPriority value.")
            .When(x => !(x.Priority is null));
    }
}
