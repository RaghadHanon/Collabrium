using API.Contracts.WorkItems;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems;

public class ChangeWorkItemStatusRequestValidator : AbstractValidator<ChangeWorkItemStatusRequest>
{
    public ChangeWorkItemStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .Must(value => Enum.TryParse<WorkItemStatus>(value.ToString(), true, out _))
            .WithMessage("NewStatus must be a valid WorkItemStatus value.");
    }
}