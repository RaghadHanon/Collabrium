using Application.Features.AI.DTOs.SkillGaps;

namespace Application.Interfaces.AI;
public interface IUserSkillGapAiService
{
    Task<SkillGapAnalysisDto> AnalyzeAsync(
        string role,
        IReadOnlyCollection<string> userSkills,
        CancellationToken cancellationToken = default);
}