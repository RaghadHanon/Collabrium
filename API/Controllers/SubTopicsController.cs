using API.Contracts.SubTopics;
using API.SetupProgram;
using Application.Features.Roadmaps.DTOs;
using Application.Features.SubTopics.Commands;
using Application.Features.SubTopics.DTOs;
using Application.Features.SubTopics.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics")]
    [ApiController]
    [Authorize(Policy = Policies.PasswordChanged)]
    public sealed class SubTopicsController : ApiBaseController
    {
        /// <summary>
        /// Retrieves subtopics for a topic node (for roadmap tree UI).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <returns>List of subtopics.</returns>
        /// <response code="200">Subtopics retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic not found.</response>
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

        /// <summary>
        /// Retrieves a subtopic by id (basic DTO).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/{subTopicId}
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <param name="subTopicId">The subtopic id (from route).</param>
        /// <returns>The subtopic.</returns>
        /// <response code="200">Subtopic retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic/subtopic not found.</response>
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

        /// <summary>
        /// Retrieves subtopic details (for sidebar UI), including resources list.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/{subTopicId}/details
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <param name="subTopicId">The subtopic id (from route).</param>
        /// <returns>Subtopic details with resources.</returns>
        /// <response code="200">Subtopic details loaded successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic/subtopic not found.</response>
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

        /// <summary>
        /// Creates a new subtopic under a topic node (scoped to a roadmap).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/subtopic
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "Indexes",
        ///       "description": "Learn how indexes work",
        ///       "order": 1
        ///     }
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <param name="request">Subtopic creation payload.</param>
        /// <returns>The created subtopic.</returns>
        /// <response code="201">Subtopic created successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic not found.</response>
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

        /// <summary>
        /// Updates an existing subtopic (title/description/order).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/subtopic/{subTopicId}
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "Indexes - Updated",
        ///       "description": "Updated description",
        ///       "order": 2
        ///     }
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <param name="subTopicId">The subtopic id (from route).</param>
        /// <param name="request">Update payload.</param>
        /// <returns>The updated subtopic.</returns>
        /// <response code="200">Subtopic updated successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic/subtopic not found.</response>
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

        /// <summary>
        /// Deletes a subtopic.
        /// Rule: cannot delete a subtopic that has resources.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/roadmaps/roadmap/{roadmapId}/topics/{topicNodeId}/subtopics/subtopic/{subTopicId}
        ///
        /// </remarks>
        /// <param name="roadmapId">The roadmap id (from route).</param>
        /// <param name="topicNodeId">The topic node id (from route).</param>
        /// <param name="subTopicId">The subtopic id (from route).</param>
        /// <response code="200">Subtopic deleted successfully.</response>
        /// <response code="400">If subtopic has resources.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If tenant/roadmap/topic/subtopic not found.</response>
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
}

