using API.Contracts.WorkItems;
using FluentValidation;

namespace API.Validators.WorkItems;

public class GetWorkItemsRequestValidator : AbstractValidator<GetWorkItemsRequest>
{
    public GetWorkItemsRequestValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue)
            .WithMessage("RelationType must be a valid WorkItemType value.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Status must be a valid WorkItemStatus value.");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority.HasValue)
            .WithMessage("Priority must be a valid WorkItemPriority value.");

        RuleFor(x => x.TitleContains)
            .MaximumLength(200).WithMessage("TitleContains cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.TitleContains));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x)
            .Must(r => !(r.CreatedAfter.HasValue && r.CreatedBefore.HasValue) || r.CreatedAfter <= r.CreatedBefore)
            .WithMessage("CreatedAfter must be less than or equal to CreatedBefore.");

        RuleFor(x => x.Cursor)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.Cursor.HasValue)
            .WithMessage("Cursor cannot be in the future.");
    }
}