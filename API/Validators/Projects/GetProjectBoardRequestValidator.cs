using API.Contracts.WorkItems;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Projects;

public class GetProjectBoardRequestValidator : AbstractValidator<GetProjectBoardRequest>
{
    public GetProjectBoardRequestValidator()
    {
        RuleFor(x => x.Type)
            .Must(type => Enum.IsDefined(typeof(WorkItemType), type))
            .When(x => !string.IsNullOrWhiteSpace(x.Type))
            .WithMessage("RelationType must be a valid WorkItemType value.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Status must be a valid WorkItemStatus value.");

        RuleFor(x => x.Priority)
            .Must(p => Enum.IsDefined(typeof(WorkItemPriority), p))
            .When(x => x.Priority.HasValue)
            .WithMessage("Priority must be a valid WorkItemPriority value.");

        RuleFor(x => x.SprintId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.SprintId is not null)
            .WithMessage("SprintId cannot be empty.");

        RuleFor(x => x.AssignedToUserId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.AssignedToUserId is not null)
            .WithMessage("AssignedToUserId cannot be empty.");

        RuleFor(x => x.TitleContains)
            .MaximumLength(200).WithMessage("TitleContains cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.TitleContains));

        RuleFor(x => x)
            .Must(r => !(r.CreatedAfter.HasValue && r.CreatedBefore.HasValue) || r.CreatedAfter <= r.CreatedBefore)
            .WithMessage("CreatedAfter must be less than or equal to CreatedBefore.");
    }
}