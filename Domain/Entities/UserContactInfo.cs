namespace Domain.Entities;
public class UserContactInfo : ContactInfo
{
    public string? Github { get; set; } = string.Empty;
    //Navigation property
    public string? UserId { get; set; }
}
