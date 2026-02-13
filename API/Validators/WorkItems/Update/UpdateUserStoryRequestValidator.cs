using API.Contracts.WorkItems.Update;
using FluentValidation;

namespace API.Validators.WorkItems.Update;

public sealed class UpdateUserStoryRequestValidator : AbstractValidator<UpdateUserStoryRequest>
{
    public UpdateUserStoryRequestValidator()
    {
        Include(new UpdateWorkItemRequestValidator());

        RuleFor(x => x.StoryPoints)
            .GreaterThanOrEqualTo(0).WithMessage("StoryPoints must be greater than or equal to 0.")
            .When(x => x.StoryPoints.HasValue);

        RuleFor(x => x.StoryPoints)
            .LessThanOrEqualTo(100).WithMessage("StoryPoints is too high.")
            .When(x => x.StoryPoints.HasValue);

        RuleFor(x => x)
             .Must(HasAnyField)
             .WithMessage("At least one field must be provided to update the work item.");
    }

    private static bool HasAnyField(UpdateUserStoryRequest r)
    {
        return !(r.Title is null)
        || !(r.Description is null)
        || !(r.AcceptanceCriteria is null)
        || r.EffortPoints.HasValue
        || !(r.Priority is null)
        || r.SprintId is not null
        || r.IsBlocked.HasValue
        || r.StoryPoints.HasValue;  
    }
}