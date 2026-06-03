using API.Contracts.AI;
using API.SetupProgram;
using Application.Features.AI.DTOs.Roadmaps;
using Application.Features.AI.Queries.Roadmaps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/ai/roadmaps")]
[ApiController]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class AIRoadmapsController : ApiBaseController
{

    [Authorize(Policy = Policies.Roadmaps_Manage)]
    [HttpPost("full-draft")]
    public async Task<ActionResult<RoadmapFullDraftDto>> GenerateFullDraft(
         [FromBody] GenerateRoadmapFullDraftRequest request)
    {
        var query = Mapper.Map<GenerateRoadmapFullDraft.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }
    [Authorize(Policy = Policies.Roadmaps_Manage)]
    [HttpPost("{roadmapId}/topics/suggestions")]
    public async Task<ActionResult<RoadmapTopicSuggestionsDto>> GenerateTopicSuggestions(
       string roadmapId,
       [FromBody] GenerateTopicSuggestionsRequest request)
    {
        var query = Mapper.Map<GenerateTopicSuggestions.Query>(request);
        query.RoadmapId = roadmapId;

        return HandleResult(await Mediator.Send(query));
    }

    [Authorize(Policy = Policies.Roadmaps_Manage)]
    [HttpPost("{roadmapId}/topics/{topicNodeId}/subtopics/suggestions")]
    public async Task<ActionResult<RoadmapSubTopicSuggestionsDto>> GenerateSubTopicSuggestions(
        string roadmapId,
        string topicNodeId,
        [FromBody] GenerateSubTopicSuggestionsRequest request)
    {
        var query = Mapper.Map<GenerateSubTopicSuggestions.Query>(request);
        query.RoadmapId = roadmapId;
        query.TopicNodeId = topicNodeId;

        return HandleResult(await Mediator.Send(query));
    }

    [Authorize(Policy = Policies.Roadmaps_Manage)]
    [HttpPost("{roadmapId}/topics/{topicNodeId}/subtopics/{subTopicId}/resources/suggestions")]
    public async Task<ActionResult<RoadmapResourceSuggestionsDto>> GenerateResourceSuggestions(
        string roadmapId,
        string topicNodeId,
        string subTopicId,
        [FromBody] GenerateResourceSuggestionsRequest request)
    {
        var query = Mapper.Map<GenerateResourceSuggestions.Query>(request);
        query.RoadmapId = roadmapId;
        query.TopicNodeId = topicNodeId;
        query.SubTopicId = subTopicId;

        return HandleResult(await Mediator.Send(query));
    }
}