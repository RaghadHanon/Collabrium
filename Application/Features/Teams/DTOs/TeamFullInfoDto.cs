using Application.Features.Projects.DTOs;
using Application.Features.Users.DTOs;

namespace Application.Features.Teams.DTOs;
public class TeamFullInfoDto : TeamDto
{
    public required UserDto TeamLead { get; set; }
    public IReadOnlyList<UserDto> Members { get; set; } = new List<UserDto>();
    public IReadOnlyList<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
}
