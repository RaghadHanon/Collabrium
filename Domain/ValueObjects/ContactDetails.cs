namespace Domain.ValueObjects;

public sealed record ContactDetails(string Email, string PhoneNumber, SocialLinks Socials)
{
    public static ContactDetails Empty => new("", "", SocialLinks.Empty);

    public static ContactDetails Create(string? email, string? phoneNumber, SocialLinks socials)
    {
        return new(email?.Trim() ?? "", phoneNumber?.Trim() ?? "", socials);
    }
}