using API.Contracts.RoadmapResources;
using API.SetupProgram;
using Application.Features.RoadmapResources.Commands;
using Application.Features.RoadmapResources.DTOs;
using Application.Features.RoadmapResources.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/{subTopicId}/resources")]
    [ApiController]
    [Authorize(Policy = Policies.PasswordChanged)]
    public sealed class RoadmapResourcesController : ApiBaseController
    {
        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet]
        public async Task<ActionResult<List<ResourceDto>>> GetResources(
            string roadmapId,
            string topicNodeId,
            string subTopicId)
        {
            var query = new GetResourcesBySubTopic.Query
            {
                RoadmapId = roadmapId,
                TopicNodeId = topicNodeId,
                SubTopicId = subTopicId
            };

            return HandleResult(await Mediator.Send(query));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet("{resourceId}")]
        public async Task<ActionResult<ResourceDto>> GetResource(
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            string resourceId)
        {
            var query = new GetResourceById.Query
            {
                RoadmapId = roadmapId,
                TopicNodeId = topicNodeId,
                SubTopicId = subTopicId,
                ResourceId = resourceId
            };

            return HandleResult(await Mediator.Send(query));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPost("resource")]
        public async Task<ActionResult<ResourceDto>> CreateResource(
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            CreateResourceRequest request)
        {
            var command = Mapper.Map<CreateResource.Command>(request);
            command.RoadmapId = roadmapId;
            command.TopicNodeId = topicNodeId;
            command.SubTopicId = subTopicId;

            return HandleResult(await Mediator.Send(command), "GetResource", "Resources");
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPatch("resource/{resourceId}")]
        public async Task<ActionResult<ResourceDto>> UpdateResource(
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            string resourceId,
            UpdateResourceRequest request)
        {
            var command = Mapper.Map<UpdateResource.Command>(request);
            command.RoadmapId = roadmapId;
            command.TopicNodeId = topicNodeId;
            command.SubTopicId = subTopicId;
            command.ResourceId = resourceId;

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPatch("resource/{resourceId}/move-up")]
        public async Task<ActionResult<ResourceDto>> MoveResourceUp(
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            string resourceId)
        {
            var command = new MoveResourceUp.Command
            {
                RoadmapId = roadmapId,
                TopicNodeId = topicNodeId,
                SubTopicId = subTopicId,
                ResourceId = resourceId
            };

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPatch("resource/{resourceId}/move-down")]
        public async Task<ActionResult<ResourceDto>> MoveResourceDown(
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            string resourceId)
        {
            var command = new MoveResourceDown.Command
            {
                RoadmapId = roadmapId,
                TopicNodeId = topicNodeId,
                SubTopicId = subTopicId,
                ResourceId = resourceId
            };

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpDelete("resource/{resourceId}")]
        public async Task<ActionResult> DeleteResource(
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            string resourceId)
        {
            var command = new DeleteResource.Command
            {
                RoadmapId = roadmapId,
                TopicNodeId = topicNodeId,
                SubTopicId = subTopicId,
                ResourceId = resourceId
            };

            return HandleResult(await Mediator.Send(command));
        }
    }
}
