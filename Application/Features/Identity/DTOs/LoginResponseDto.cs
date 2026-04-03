namespace Application.Features.Identity.DTOs;

/// <summary>
/// Data Transfer Object for login response containing access token, refresh token, user ID, and tenant ID
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// JWT Access token issued upon successful authentication
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// JWT Refresh token issued alongside the access token, used to obtain new access tokens without re-authenticating
    /// </summary>
    public required string RefreshToken { get; set; }

    /// <summary>
    /// Unique identifier of the authenticated user
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Unique identifier of the tenant that the user belongs to
    /// </summary>
    public required string TenantId { get; set; }
}