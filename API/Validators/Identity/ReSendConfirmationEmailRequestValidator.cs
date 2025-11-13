using API.Contracts.Identity;
using FluentValidation;

namespace API.Validators.Identity;

public class ReSendConfirmationEmailRequestValidator : AbstractValidator<ReSendConfirmationEmailRequest>
{
    public ReSendConfirmationEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");
    }
}
