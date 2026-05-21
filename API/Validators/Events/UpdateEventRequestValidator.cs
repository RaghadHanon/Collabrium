using API.Contracts.Events;
using FluentValidation;

namespace API.Validators.Events;

public sealed class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description is not null);

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt)
            .When(x =>( x.StartAt.HasValue )&& (x.EndAt.HasValue))
            .WithMessage("EndAt must be after StartAt.");

        RuleFor(x => x.Location)
            .MaximumLength(500)
            .When(x => x.Location is not null);

        RuleFor(x => x.MeetingUrl)
            .MaximumLength(1000)
            .When(x => x.MeetingUrl is not null);

        RuleFor(x => x.MaxAttendees)
            .GreaterThan(0)
            .When(x => x.MaxAttendees.HasValue);
    }
}