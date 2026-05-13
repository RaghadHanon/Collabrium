using Domain.Enums;

namespace API.Contracts.Identity;

public class RegisterEmployeeRequest : RegisterAppUserRequest
{
    /// <summary>
    /// Gets or sets the role assigned to the user.
    /// </summary>
    public required UserRole UserRole { get; set; }
    /// <summary>
    /// Gets or sets the department associated with the current entity.
    /// </summary>
    public required Department Department { get; set; }
}
