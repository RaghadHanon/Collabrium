using API.Contracts.WorkItems.Create;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems.Create;

public sealed class CreateTestCaseRequestValidator : AbstractValidator<CreateTestCaseRequest>
{
    public CreateTestCaseRequestValidator()
    {
        Include(new CreateWorkItemRequestValidator());

        RuleFor(x => x.Preconditions)
            .NotEmpty().WithMessage("Preconditions is required.")
            .MaximumLength(4000).WithMessage("Preconditions cannot exceed 4000 characters.");

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("Steps is required.")
            .MaximumLength(4000).WithMessage("Steps cannot exceed 4000 characters.");

        RuleFor(x => x.ExpectedResult)
            .NotEmpty().WithMessage("ExpectedResult is required.")
            .MaximumLength(4000).WithMessage("ExpectedResult cannot exceed 4000 characters.");

        RuleFor(x => x.TestEnvironment)
            .NotEmpty().WithMessage("TestEnvironment is required.")
            .Must(v => Enum.TryParse<TestEnvironment>(v, true, out _))
            .WithMessage("TestEnvironment must be a valid TestEnvironment value.");
    }
}
