using API.Contracts.WorkItems.Update;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems.Update;

public sealed class UpdateTestCaseRequestValidator : AbstractValidator<UpdateTestCaseRequest>
{
    public UpdateTestCaseRequestValidator()
    {
        Include(new UpdateWorkItemRequestValidator());

        RuleFor(x => x.Preconditions)
            .MaximumLength(4000).WithMessage("Preconditions cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Preconditions));

        RuleFor(x => x.Steps)
            .MaximumLength(4000).WithMessage("Steps cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Steps));

        RuleFor(x => x.ExpectedResult)
            .MaximumLength(4000).WithMessage("ExpectedResult cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.ExpectedResult));

        RuleFor(x => x.ActualResult)
            .MaximumLength(4000).WithMessage("ActualResult cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.ActualResult));

        RuleFor(x => x.TestEnvironment)
            .IsInEnum()
            .WithMessage("TestEnvironment must be a valid TestEnvironment value.")
            .When(x => !(x.TestEnvironment is null));

        RuleFor(x => x.Result)
            .IsInEnum()
            .WithMessage("Result must be a valid Result value.")
            .When(x => !(x.Result is null));

        RuleFor(x => x)
            .Must(HasAnyField)
            .WithMessage("At least one field must be provided to update the work item.");
    }

    private static bool HasAnyField(UpdateTestCaseRequest r)
    {
        return !(r.Title is null)
        || !(r.Description is null)
        || !(r.AcceptanceCriteria is null)
        || r.EffortPoints.HasValue
        || !(r.Priority is null)
        || !(r.SprintId is null)
        || r.IsBlocked.HasValue
        || !(r.Preconditions is null)
        || !(r.Steps is null)
        || !(r.ExpectedResult is null)
        || !(r.ActualResult is null)
        || !(r.TestEnvironment is null)
        || !(r.Result is null);
    }
}