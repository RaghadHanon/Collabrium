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
        /// <summary>
        /// Creates a new topic node within the current tenant (scoped to a roadmap).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/roadmaps/roadmap/{roadmapId}/nodes/topic
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "Databases",
        ///       "description": "Learn relational databases fundamentals",
        ///       "order": 1
        ///     }
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="request">Topic node creation payload.</param>
        /// <returns>The created topic node.</returns>
        /// <response code="201">Topic node created successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant or roadmap is not found.</response>

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPost("topic")]
        public async Task<ActionResult<TopicNodeDto>> CreateTopic(
            string roadmapId,
            CreateTopicNodeRequest request)
        {
            var command = Mapper.Map<CreateTopicNode.Command>(request);
            command.RoadmapId = roadmapId;

            return HandleResult(await Mediator.Send(command), "GetTopic", "Topics");
        }

        /// <summary>
        /// Creates a new reference node within the current tenant (scoped to a roadmap).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/roadmaps/roadmap/{roadmapId}/nodes/reference
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "Frontend Roadmap",
        ///       "description": "Reference to a full roadmap",
        ///       "order": 2,
        ///       "refRoadmapId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        ///     }
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="request">Reference node creation payload.</param>
        /// <returns>The created reference node.</returns>
        /// <response code="201">Reference node created successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant, roadmap, or referenced roadmap is not found.</response>

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPost("reference")]
        public async Task<ActionResult<ReferenceNodeDto>> CreateReference(
            string roadmapId,
            CreateReferenceNodeRequest request)
        {
            var command = Mapper.Map<CreateReferenceNode.Command>(request);
            command.RoadmapId = roadmapId;

            return HandleResult(await Mediator.Send(command), "GetReference", "References");
        }
        /// <summary>
        /// Retrieves all roadmap nodes (topic + reference) for a roadmap (scoped to current tenant).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/roadmaps/roadmap/{roadmapId}/nodes
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <returns>List of roadmap nodes.</returns>
        /// <response code="200">Nodes retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read roadmaps.</response>
        /// <response code="404">If the current tenant or roadmap is not found.</response>
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
        /// <summary>
        /// Retrieves a roadmap node by id (topic or reference) (scoped to current tenant).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/roadmaps/roadmap/{roadmapId}/nodes/{nodeId}
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="nodeId">The node id (from route).</param>
        /// <returns>The node details.</returns>
        /// <response code="200">Node retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read roadmaps.</response>
        /// <response code="404">If the current tenant, roadmap, or node is not found.</response>

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
        /// <summary>
        /// Updates an existing topic node (title/description/order).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/roadmaps/roadmap/{roadmapId}/nodes/topic/{nodeId}
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "Databases - Updated",
        ///       "description": "Updated description",
        ///       "order": 1
        ///     }
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="nodeId">The topic node id (from route).</param>
        /// <param name="request">Update payload.</param>
        /// <returns>The updated topic node.</returns>
        /// <response code="200">Topic node updated successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant, roadmap, or node is not found.</response>

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

        /// <summary>
        /// Updates an existing reference node (title/description/order/refRoadmapId).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/roadmaps/roadmap/{roadmapId}/nodes/reference/{nodeId}
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "Frontend Roadmap - Updated",
        ///       "description": "Updated description",
        ///       "order": 2,
        ///       "refRoadmapId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        ///     }
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="nodeId">The reference node id (from route).</param>
        /// <param name="request">Update payload.</param>
        /// <returns>The updated reference node.</returns>
        /// <response code="200">Reference node updated successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant, roadmap, node, or referenced roadmap is not found.</response>

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
        /// <summary>
        /// Deletes a roadmap node by id (topic or reference).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/roadmaps/roadmap/{roadmapId}/nodes/{nodeId}
        ///
        /// Notes:
        /// - Topic nodes cannot be deleted if they have subtopics.
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="nodeId">The node id (from route).</param>
        /// <response code="200">Node deleted successfully.</response>
        /// <response code="400">If trying to delete a topic node that has subtopics.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant, roadmap, or node is not found.</response>
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


