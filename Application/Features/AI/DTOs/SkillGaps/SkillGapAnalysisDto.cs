namespace Application.Features.AI.DTOs.SkillGaps;
public sealed class SkillGapAnalysisDto
{
    public string Role { get; set; } = string.Empty;

    public int ReadinessScore { get; set; }

    public string Summary { get; set; } = string.Empty;

    public List<string> Strengths { get; set; } = new();

    public List<MissingSkillDto> MissingSkills { get; set; } = new();

    public string TopPrioritySkill { get; set; } = string.Empty;

    public string ReadinessLevel { get; set; } = string.Empty;

    public string FinalAdvice { get; set; } = string.Empty;
}