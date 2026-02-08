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
            .When(x => !string.IsNullOrWhiteSpace(x.Preconditions));

        RuleFor(x => x.Steps)
            .MaximumLength(4000).WithMessage("Steps cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Steps));

        RuleFor(x => x.ExpectedResult)
            .MaximumLength(4000).WithMessage("ExpectedResult cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ExpectedResult));

        RuleFor(x => x.ActualResult)
            .MaximumLength(4000).WithMessage("ActualResult cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ActualResult));

        RuleFor(x => x.TestEnvironment)
            .Must(v => Enum.TryParse<TestEnvironment>(v, true, out _))
            .WithMessage("TestEnvironment must be a valid TestEnvironment value.")
            .When(x => !string.IsNullOrWhiteSpace(x.TestEnvironment));

        RuleFor(x => x.Result)
            .Must(v => Enum.TryParse<TestCaseResult>(v, true, out _))
            .WithMessage("Result must be a valid Result value.")
            .When(x => !string.IsNullOrWhiteSpace(x.Result));
    }
}