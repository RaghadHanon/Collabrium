using API.Contracts.WorkItems.Create;
using FluentValidation;

namespace API.Validators.WorkItems.Create;

public sealed class CreateBugRequestValidator : AbstractValidator<CreateBugRequest>
{
    public CreateBugRequestValidator()
    {
        Include(new CreateWorkItemRequestValidator());

        RuleFor(x => x.StepsToReproduce)
            .NotEmpty().WithMessage("StepsToReproduce is required.")
            .MaximumLength(4000).WithMessage("StepsToReproduce cannot exceed 4000 characters.");
    }
}