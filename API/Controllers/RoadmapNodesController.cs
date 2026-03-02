using API.Contracts.RoadmapNodes;
using API.SetupProgram;
using Application.Features.RoadmapNodes.Commands;
using Application.Features.RoadmapNodes.DTOs;
using Application.Features.RoadmapNodes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/roadmaps/roadmap/{roadmapId}/nodes")]
    [ApiController]
    [Authorize(Policy = Policies.PasswordChanged)]
    public sealed class RoadmapNodesController : ApiBaseController
    {

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPost("topic")]
        public async Task<ActionResult<TopicNodeDto>> CreateTopic(
            string roadmapId,
            CreateTopicNodeRequest request)
        {
            var command = Mapper.Map<CreateTopicNode.Command>(request);
            command.RoadmapId = roadmapId;

            return HandleResult(await Mediator.Send(command));
        }


        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPost("reference")]
        public async Task<ActionResult<ReferenceNodeDto>> CreateReference(
            string roadmapId,
            CreateReferenceNodeRequest request)
        {
            var command = Mapper.Map<CreateReferenceNode.Command>(request);
            command.RoadmapId = roadmapId;

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet]
        public async Task<ActionResult<List<RoadmapNodeDto>>> GetNodes(string roadmapId)
        {
            var query = new GetRoadmapNodes.Query
            {
                RoadmapId = roadmapId
            };

            return HandleResult(await Mediator.Send(query));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet("{nodeId}")]
        public async Task<ActionResult<RoadmapNodeDto>> GetNode(
            string roadmapId,
            string nodeId)
        {
            var query = new GetRoadmapNodeById.Query
            {
                RoadmapId = roadmapId,
                NodeId = nodeId
            };

            return HandleResult(await Mediator.Send(query));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPatch("topic/{nodeId}")]
        public async Task<ActionResult<TopicNodeDto>> UpdateTopic(
            string roadmapId,
            string nodeId,
            UpdateTopicNodeRequest request)
        {
            var command = Mapper.Map<UpdateTopicNode.Command>(request);
            command.RoadmapId = roadmapId;
            command.TopicNodeId = nodeId;

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPatch("reference/{nodeId}")]
        public async Task<ActionResult<ReferenceNodeDto>> UpdateReference(
            string roadmapId,
            string nodeId,
            UpdateReferenceNodeRequest request)
        {
            var command = Mapper.Map<UpdateReferenceNode.Command>(request);
            command.RoadmapId = roadmapId;
            command.ReferenceNodeId = nodeId;

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpDelete("{nodeId}")]
        public async Task<ActionResult> DeleteNode(
            string roadmapId,
            string nodeId)
        {
            var command = new DeleteRoadmapNode.Command
            {
                RoadmapId = roadmapId,
                NodeId = nodeId
            };

            return HandleResult(await Mediator.Send(command));
        }
    }
}


