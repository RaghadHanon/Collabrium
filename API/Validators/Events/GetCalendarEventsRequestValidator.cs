using API.Contracts.Events;
using FluentValidation;

namespace API.Validators.Events;

public sealed class GetCalendarEventsRequestValidator : AbstractValidator<GetCalendarEventsRequest>
{
    public GetCalendarEventsRequestValidator()
    {
        RuleFor(x => x.StartsAfter)
            .NotEmpty();

        RuleFor(x => x.StartsBefore)
            .NotEmpty()
            .GreaterThan(x => x.StartsAfter)
            .WithMessage("StartsBefore must be after StartsAfter.");
    }
}