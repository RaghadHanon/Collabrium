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
            .When(x => !string.IsNullOrWhiteSpace(x.StepsToReproduce));

        RuleFor(x => x.ExpectedResult)
            .MaximumLength(4000).WithMessage("ExpectedResult cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ExpectedResult));

        RuleFor(x => x.ActualResult)
            .MaximumLength(4000).WithMessage("ActualResult cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ActualResult));
    }
}