namespace Infrastructure.Settings;

public class IdentitySettings
{
    public required string ConfirmationUrlBase { get; set; }
    public required string PasswordResetUrlBase { get; set; }
}