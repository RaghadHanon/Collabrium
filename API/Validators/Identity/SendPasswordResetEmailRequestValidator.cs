using API.Contracts.Identity;
using FluentValidation;

namespace API.Validators.Identity;

public class SendPasswordResetEmailRequestValidator : AbstractValidator<SendPasswordResetEmailRequest>
{
    public SendPasswordResetEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");
    }
}
