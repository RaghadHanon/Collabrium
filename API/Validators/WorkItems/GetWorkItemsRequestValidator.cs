using API.Contracts.WorkItems;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems;

public class GetWorkItemsRequestValidator : AbstractValidator<GetWorkItemsRequest>
{
    public GetWorkItemsRequestValidator()
    {
        RuleFor(x => x.Type)
            .Must(type => Enum.IsDefined(typeof(WorkItemType), type))
            .When(x => !string.IsNullOrWhiteSpace(x.Type))
            .WithMessage("RelationType must be a valid WorkItemType value.");

        RuleFor(x => x.Status)
            .Must(status => Enum.IsDefined(typeof(WorkItemStatus), status))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be a valid WorkItemStatus value.");

        RuleFor(x => x.Priority)
            .Must(p => Enum.IsDefined(typeof(WorkItemPriority), p))
            .When(x => !string.IsNullOrWhiteSpace(x.Priority))
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

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x)
            .Must(r => !(r.CreatedAfter.HasValue && r.CreatedBefore.HasValue) || r.CreatedAfter <= r.CreatedBefore)
            .WithMessage("CreatedAfter must be less than or equal to CreatedBefore.");

        RuleFor(x => x.Cursor)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.Cursor.HasValue)
            .WithMessage("Cursor cannot be in the future.");
    }
}