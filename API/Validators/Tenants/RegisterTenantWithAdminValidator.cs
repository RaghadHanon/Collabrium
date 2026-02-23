using API.Contracts.Tenants;
using API.Validators.Identity;
using FluentValidation;

namespace API.Validators.Tenants;

/// <summary>
/// Validator for IT registration, inherits all rules from RegisterAppUserValidator.
/// </summary>
public class RegisterTenantWithAdminValidator : RegisterAppUserValidator<RegisterTenantWithAdminRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterTenantWithAdminValidator"/> class.
    /// </summary>
    public RegisterTenantWithAdminValidator()
    {
        RuleFor(x => x.TenantName)
                    .NotEmpty().WithMessage("Tenant name is required.")
                    .MaximumLength(100).WithMessage("Tenant name cannot exceed 100 characters.");

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage("Domain is required.")
            .MaximumLength(100).WithMessage("Domain cannot exceed 100 characters.")
            .Matches(@"^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Domain))
            .WithMessage("Domain must be a valid domain format (e.g., example.com).");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Description cannot exceed 500 characters.");
    }
}