using API.Contracts.WorkItems.Create;
using FluentValidation;

namespace API.Validators.WorkItems.Create;

public sealed class CreateWorkItemRequestValidator : AbstractValidator<CreateWorkItemRequest>
{
    public CreateWorkItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Priority must be a valid WorkItemPriority value.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.AcceptanceCriteria)
            .MaximumLength(4000).WithMessage("AcceptanceCriteria cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.AcceptanceCriteria));

        RuleFor(x => x.EffortPoints)
            .GreaterThanOrEqualTo(0).WithMessage("EffortPoints must be greater than or equal to 0.");
    }
}