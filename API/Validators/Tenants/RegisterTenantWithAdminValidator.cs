using API.Contracts.Tenants;
using API.Validators.Identity;
using FluentValidation;

namespace API.Validators.Tenants;

/// <summary>
/// Validator for OrganizationIT registration, inherits all rules from RegisterAppUserValidator.
/// </summary>
public class RegisterTenantWithAdminValidator : RegisterAppUserValidator<RegisterTenantWithAdminRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterTenantWithAdminValidator"/> class.
    /// </summary>
    public RegisterTenantWithAdminValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant Name is required.")
            .MaximumLength(50).WithMessage("Tenant Name cannot exceed 50 characters.");

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage("Tenant Domain is required.")
            .MaximumLength(50).WithMessage("Tenant Domain cannot exceed 50 characters.");
    }
}