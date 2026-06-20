using API.Contracts.Identity;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Identity;

/// <summary>
/// Validator for child registration, inherits all rules from RegisterAppUserValidator and adds Difficulty rules.
/// </summary>
public class RegisterEmployeeValidator : RegisterAppUserValidator<RegisterEmployeeRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEmployeeValidator"/> class.
    /// </summary>
    public RegisterEmployeeValidator()
    {
        RuleFor(x => x.UserRole)
            .IsInEnum()
            .WithMessage("User Role is required.")
            .Must(value => value is not (UserRole.SuperAdmin))
            .WithMessage("This role is not allowed for registration.");

        RuleFor(x => x.Department)
           .IsInEnum()
           .WithMessage("Department  is required.");
    }
}