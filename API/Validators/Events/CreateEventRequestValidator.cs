using API.Contracts.Events;
using FluentValidation;

namespace API.Validators.Events;

public sealed class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.StartAt)
            .NotEmpty();

        RuleFor(x => x.EndAt)
            .NotEmpty()
            .GreaterThan(x => x.StartAt)
            .WithMessage("EndAt must be after StartAt.");

        RuleFor(x => x.Location)
            .MaximumLength(500);

        RuleFor(x => x.MeetingUrl)
            .MaximumLength(1000);

        RuleFor(x => x.MeetingUrl)
            .NotEmpty()
            .When(x => x.IsOnline)
            .WithMessage("MeetingUrl is required for online events.");

        RuleFor(x => x.Location)
            .NotEmpty()
            .When(x => !x.IsOnline)
            .WithMessage("Location is required for offline events.");

        RuleFor(x => x.MaxAttendees)
            .GreaterThan(0)
            .When(x => x.MaxAttendees.HasValue);
    }
}