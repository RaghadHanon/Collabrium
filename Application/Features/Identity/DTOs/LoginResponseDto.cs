namespace Application.Features.Identity.DTOs;

/// <summary>
/// Data Transfer Object for login response containing JWT token and user ID
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// JWT token issued upon successful authentication
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Unique identifier of the authenticated user
    /// </summary>
    public required string Id { get; set; }
    /// <summary>
    /// Unique identifier of the tenant that the user belongs to 
    /// </summary>
    public required string TenantId { get; set; }
}