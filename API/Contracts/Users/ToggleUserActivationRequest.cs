namespace API.Contracts.Users;

public sealed class ToggleUserActivationRequest
{
    public bool IsActive { get; set; }
}