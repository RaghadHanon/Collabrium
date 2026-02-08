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
            .NotEmpty().WithMessage("User Role is required.")
            .Must(value => Enum.TryParse<UserRole>(value, true, out _))
            .WithMessage("User Role is invalid.")
            .Must(value =>
            {
                if (!Enum.TryParse<UserRole>(value, true, out var role))
                    return false;

                return role is not (UserRole.TenantAdmin or UserRole.SuperAdmin);
            })
            .WithMessage("This role is not allowed for registration.");

        RuleFor(x => x.Department)
           .NotEmpty().WithMessage("Department  is required.")
           .Must(value => Enum.TryParse<Department>(value, true, out _))
           .WithMessage("Department is invalid.");
    }
}