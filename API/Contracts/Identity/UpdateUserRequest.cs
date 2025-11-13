namespace API.Contracts.Identity;

/// <summary>
/// Request model for updating an existing user.
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// User's first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// User's date of birth.
    /// </summary>
    public DateOnly DateOfBirth { get; set; }
}