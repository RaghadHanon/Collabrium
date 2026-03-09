using API.Contracts.WorkItems.Update;
using FluentValidation;

namespace API.Validators.WorkItems.Update;

public sealed class UpdateBugRequestValidator : AbstractValidator<UpdateBugRequest>
{
    public UpdateBugRequestValidator()
    {
        Include(new UpdateWorkItemRequestValidator());

        RuleFor(x => x.StepsToReproduce)
            .MaximumLength(4000).WithMessage("StepsToReproduce cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.StepsToReproduce));

        RuleFor(x => x.ExpectedResult)
            .MaximumLength(4000).WithMessage("ExpectedResult cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.ExpectedResult));

        RuleFor(x => x.ActualResult)
            .MaximumLength(4000).WithMessage("ActualResult cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.ActualResult));

        RuleFor(x => x)
            .Must(HasAnyField)
            .WithMessage("At least one field must be provided to update the work item.");
    }

    private static bool HasAnyField(UpdateBugRequest r)
    {
        return !(r.Title is null)
        || !(r.Description is null)
        || !(r.AcceptanceCriteria is null)
        || r.EffortPoints.HasValue
        || !(r.Priority is null)
        || !(r.SprintId is null)
        || r.IsBlocked.HasValue
        || !(r.StepsToReproduce is null)
        || !(r.ExpectedResult is null)
        || !(r.ActualResult is null);
    }
}