using API.Contracts.WorkItems.Comments;
using FluentValidation;

namespace API.Validators.WorkItems.Comments;

public class GetWorkItemCommentsRequestValidator : AbstractValidator<GetWorkItemCommentsRequest>
{
    public GetWorkItemCommentsRequestValidator()
    {
        RuleFor(x => x.PageSize)
             .InclusiveBetween(1, 100)
             .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.Cursor)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.Cursor.HasValue)
            .WithMessage("Cursor cannot be in the future.");
    }
}