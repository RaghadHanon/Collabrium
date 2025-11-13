using API.Contracts.Identity;

namespace API.Validators.Identity;

/// <summary>
/// Validator for super admin registration, inherits all rules from RegisterAppUserValidator.
/// </summary>
public class RegisterSuperAdminRequestValidator : RegisterAppUserValidator<RegisterSuperAdminRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterSuperAdminRequestValidator"/> class.
    /// </summary>
    public RegisterSuperAdminRequestValidator()
    {
    }
}