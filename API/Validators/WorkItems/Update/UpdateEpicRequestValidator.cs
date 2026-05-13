using API.Contracts.WorkItems.Update;
using FluentValidation;

namespace API.Validators.WorkItems.Update;

public sealed class UpdateEpicRequestValidator : AbstractValidator<UpdateEpicRequest>
{
    public UpdateEpicRequestValidator()
    {
        Include(new UpdateWorkItemRequestValidator());

        RuleFor(x => x)
            .Must(HasAnyField)
            .WithMessage("At least one field must be provided to update the work item.");
    }

    private static bool HasAnyField(UpdateEpicRequest r)
    {
        return !(r.Title is null)
        || !(r.Description is null)
        || !(r.AcceptanceCriteria is null)
        || r.EffortPoints.HasValue
        || !(r.Priority is null)
        || !(r.SprintId is null)
        || r.IsBlocked.HasValue;
    }
}
