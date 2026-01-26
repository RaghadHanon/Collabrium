namespace Application.Common.DTOs;

public sealed class ContactDetailsDto
{
    public string Email { get; init; } = "";
    public string PhoneNumber { get; init; } = "";
    public SocialLinksDto Socials { get; init; } = new();
}
