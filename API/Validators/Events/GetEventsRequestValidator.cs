using API.Contracts.Events;
using FluentValidation;

namespace API.Validators.Events;

public sealed class GetEventsRequestValidator : AbstractValidator<GetEventsRequest>
{
    public GetEventsRequestValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(x => x.StartsBefore)
            .GreaterThan(x => x.StartsAfter)
            .When(x => x.StartsAfter.HasValue && x.StartsBefore.HasValue)
            .WithMessage("StartsBefore must be after StartsAfter.");
    }
}