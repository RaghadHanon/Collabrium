namespace API.Contracts.Identity;
/// <summary>
/// Represents a request to register a new app user in the system.
/// </summary>
/// <remarks>This class encapsulates the necessary information required to create a user account,
/// including personal details and role-specific data. Ensure that all required fields are provided to successfully
/// register a user.</remarks>
public class RegisterAppUserRequest
{
    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public required string FullName { get; set; }
    /// <summary>
    /// Gets or sets the email address associated with the user.
    /// </summary>
    public required string Email { get; set; }
    /// <summary>
    /// Gets or sets the password required for authentication.
    /// </summary>
    public required string Password { get; set; }
    /// <summary>
    /// Gets or sets the password required for authentication.
    /// </summary>
    public required string ConfirmPassword { get; set; }
    /// <summary>
    /// Gets or sets the date of birth.
    /// </summary>
    public DateOnly DateOfBirth { get; set; }
    /// <summary>
    /// Gets or sets the phone number associated with the contact.
    /// </summary>
    public required string PhoneNumber { get; set; }
}