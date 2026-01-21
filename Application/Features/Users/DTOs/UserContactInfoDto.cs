using Application.Common.DTOs;

namespace Application.Features.Users.DTOs;
public class UserContactInfoDto : ContactInfoDto
{
    public string Github { get; set; } = null!;
}
