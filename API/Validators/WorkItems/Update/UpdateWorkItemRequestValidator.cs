using API.Contracts.WorkItems.Update;
using FluentValidation;

namespace API.Validators.WorkItems.Update;

public sealed class UpdateWorkItemRequestValidator : AbstractValidator<UpdateWorkItemRequest>
{
    public UpdateWorkItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.AcceptanceCriteria)
            .MaximumLength(4000).WithMessage("AcceptanceCriteria cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.AcceptanceCriteria));

        RuleFor(x => x.EffortPoints)
            .GreaterThanOrEqualTo(0).WithMessage("EffortPoints must be greater than or equal to 0.")
            .When(x => x.EffortPoints.HasValue);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Priority must be a valid WorkItemPriority value.")
            .When(x => !(x.Priority is null));
    }
}
