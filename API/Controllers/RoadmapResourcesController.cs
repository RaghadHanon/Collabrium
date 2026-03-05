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
        /// <summary>
        /// Retrieves resources for a subtopic.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/{subTopicId}/resources
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <param name="subTopicId">The subtopic id (from route).</param>
        /// <returns>List of resources.</returns>
        /// <response code="200">Resources retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic/subtopic not found.</response>
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

        /// <summary>
        /// Retrieves a resource by id.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/{subTopicId}/resources/{resourceId}
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <param name="subTopicId">The subtopic id (from route).</param>
        /// <param name="resourceId">The resource id (from route).</param>
        /// <returns>The resource.</returns>
        /// <response code="200">Resource retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic/subtopic/resource not found.</response>
        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet("{resourceId}")]
        public async Task<ActionResult<ResourceDto>> GetResourceById(
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

        /// <summary>
        /// Creates a new resource under a subtopic.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/{subTopicId}/resources/resource
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "EF Core - Includes",
        ///       "url": "https://learn.microsoft.com/",
        ///       "type": 1,
        ///       "order": 1
        ///     }
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <param name="subTopicId">The subtopic id (from route).</param>
        /// <param name="request">Resource creation payload.</param>
        /// <returns>The created resource.</returns>
        /// <response code="201">Resource created successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic/subtopic not found.</response>
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

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Updates an existing resource (title/url/type/order).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/{subTopicId}/resources/resource/{resourceId}
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "Updated Title",
        ///       "url": "https://example.com",
        ///       "type": 2,
        ///       "order": 3
        ///     }
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <param name="subTopicId">The subtopic id (from route).</param>
        /// <param name="resourceId">The resource id (from route).</param>
        /// <param name="request">Update payload.</param>
        /// <returns>The updated resource.</returns>
        /// <response code="200">Resource updated successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic/subtopic/resource not found.</response>
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

        /// <summary>
        /// Deletes a resource.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/{subTopicId}/resources/resource/{resourceId}
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <param name="subTopicId">The subtopic id (from route).</param>
        /// <param name="resourceId">The resource id (from route).</param>
        /// <response code="200">Resource deleted successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic/subtopic/resource not found.</response>
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
