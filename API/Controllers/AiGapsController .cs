using API.SetupProgram;
using Application.Features.AI.DTOs.SkillGaps;
using Application.Features.AI.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Route("api/Gaps")]
[ApiController]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class AiController : ApiBaseController
{
    /// <summary>
    /// Analyzes the current user's skill gaps based on their system role and current skills.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/skill-gaps/me
    ///
    /// </remarks>
    /// <returns>Detailed AI skill gap analysis.</returns>
    /// <response code="200">Skill gap analysis generated successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant, current user, or role is not found.</response>
    [Authorize(Policy = Policies.Roadmaps_Read)]
    [HttpGet("skill-gaps/me")]
    public async Task<ActionResult<SkillGapAnalysisDto>> AnalyzeMySkillGaps()
    {
        return HandleResult(
            await Mediator.Send(new AnalyzeMySkillGaps.Query()));
    }
}