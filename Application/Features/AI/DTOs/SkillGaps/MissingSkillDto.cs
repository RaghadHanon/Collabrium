namespace Application.Features.AI.DTOs.SkillGaps;
public sealed class MissingSkillDto
{
    public string Skill { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string WhyItMatters { get; set; } = string.Empty;

    public string RecommendedLevel { get; set; } = string.Empty;
}