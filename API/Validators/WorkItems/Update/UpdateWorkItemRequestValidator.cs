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
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.AcceptanceCriteria)
            .MaximumLength(4000).WithMessage("AcceptanceCriteria cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.AcceptanceCriteria));

        RuleFor(x => x.EffortPoints)
            .GreaterThanOrEqualTo(0).WithMessage("EffortPoints must be greater than or equal to 0.")
            .When(x => x.EffortPoints.HasValue);

        RuleFor(x => x.Priority)
            .Must(p => Enum.TryParse<WorkItemPriority>(p, true, out _))
            .WithMessage("Priority must be a valid WorkItemPriority value.")
            .When(x => !string.IsNullOrWhiteSpace(x.Priority));

        RuleFor(x => x.AssignedToUserId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.AssignedToUserId is not null)
            .WithMessage("AssignedToUserId cannot be empty.");

        RuleFor(x => x.SprintId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.SprintId is not null)
            .WithMessage("SprintId cannot be empty.");

        RuleFor(x => x)
            .Must(HasAnyField)
            .WithMessage("At least one field must be provided to update the work item.");
    }

    private static bool HasAnyField(UpdateWorkItemRequest r)
    {
        return !string.IsNullOrWhiteSpace(r.Title)
               || !string.IsNullOrWhiteSpace(r.Description)
               || !string.IsNullOrWhiteSpace(r.AcceptanceCriteria)
               || r.EffortPoints.HasValue
               || !string.IsNullOrWhiteSpace(r.Priority)
               || r.AssignedToUserId is not null
               || r.SprintId is not null
               || r.IsBlocked.HasValue;
    }
}
