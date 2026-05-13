using API.Contracts.SubTopics;
using API.SetupProgram;
using Application.Features.Roadmaps.DTOs;
using Application.Features.SubTopics.Commands;
using Application.Features.SubTopics.DTOs;
using Application.Features.SubTopics.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics")]
[ApiController]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class SubTopicsController : ApiBaseController
{
    [Authorize(Policy = Policies.Roadmaps_Read)]
    [HttpGet]
    public async Task<ActionResult<List<SubTopicDto>>> GetSubTopics(string roadmapId, string topicNodeId)
    {
        var query = new GetSubTopicsByTopic.Query
        {
            RoadmapId = roadmapId,
            TopicNodeId = topicNodeId
        };

        return HandleResult(await Mediator.Send(query));
    }

    [Authorize(Policy = Policies.Roadmaps_Read)]
    [HttpGet("{subTopicId}")]
    public async Task<ActionResult<SubTopicDto>> GetSubTopic(
        string roadmapId,
        string topicNodeId,
        string subTopicId)
    {
        var query = new GetSubTopicById.Query
        {
            RoadmapId = roadmapId,
            TopicNodeId = topicNodeId,
            SubTopicId = subTopicId
        };

        return HandleResult(await Mediator.Send(query));
    }

    [Authorize(Policy = Policies.Roadmaps_Read)]
    [HttpGet("{subTopicId}/details")]
    public async Task<ActionResult<SubTopicDetailsDto>> GetSubTopicDetails(
        string roadmapId,
        string topicNodeId,
        string subTopicId)
    {
        var query = new GetSubTopicDetails.Query
        {
            RoadmapId = roadmapId,
            TopicNodeId = topicNodeId,
            SubTopicId = subTopicId
        };

        return HandleResult(await Mediator.Send(query));
    }

    [Authorize(Policy = Policies.Roadmaps_Manage)]
    [HttpPost("subtopic")]
    public async Task<ActionResult<SubTopicDto>> CreateSubTopic(
        string roadmapId,
        string topicNodeId,
        CreateSubTopicRequest request)
    {
        var command = Mapper.Map<CreateSubTopic.Command>(request);
        command.RoadmapId = roadmapId;
        command.TopicNodeId = topicNodeId;

        return HandleResult(await Mediator.Send(command), "GetSubTopic", "SubTopics");
    }

    [Authorize(Policy = Policies.Roadmaps_Manage)]
    [HttpPatch("subtopic/{subTopicId}")]
    public async Task<ActionResult<SubTopicDto>> UpdateSubTopic(
        string roadmapId,
        string topicNodeId,
        string subTopicId,
        UpdateSubTopicRequest request)
    {
        var command = Mapper.Map<UpdateSubTopic.Command>(request);
        command.RoadmapId = roadmapId;
        command.TopicNodeId = topicNodeId;
        command.SubTopicId = subTopicId;

        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Policy = Policies.Roadmaps_Manage)]
    [HttpPatch("subtopic/{subTopicId}/move-up")]
    public async Task<ActionResult<SubTopicDto>> MoveSubTopicUp(
        string roadmapId,
        string topicNodeId,
        string subTopicId)
    {
        var command = new MoveSubTopicUp.Command
        {
            RoadmapId = roadmapId,
            TopicNodeId = topicNodeId,
            SubTopicId = subTopicId
        };

        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Policy = Policies.Roadmaps_Manage)]
    [HttpPatch("subtopic/{subTopicId}/move-down")]
    public async Task<ActionResult<SubTopicDto>> MoveSubTopicDown(
        string roadmapId,
        string topicNodeId,
        string subTopicId)
    {
        var command = new MoveSubTopicDown.Command
        {
            RoadmapId = roadmapId,
            TopicNodeId = topicNodeId,
            SubTopicId = subTopicId
        };

        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Policy = Policies.Roadmaps_Manage)]
    [HttpDelete("subtopic/{subTopicId}")]
    public async Task<ActionResult> DeleteSubTopic(
        string roadmapId,
        string topicNodeId,
        string subTopicId)
    {
        var command = new DeleteSubTopic.Command
        {
            RoadmapId = roadmapId,
            TopicNodeId = topicNodeId,
            SubTopicId = subTopicId
        };

        return HandleResult(await Mediator.Send(command));
    }
}

