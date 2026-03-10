using API.Contracts.Roadmaps;
using API.SetupProgram;
using Application.Common;
using Application.Features.Roadmaps.Commands;
using Application.Features.Roadmaps.DTOs;
using Application.Features.Roadmaps.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/roadmaps")]
    [ApiController]
    [Authorize(Policy = Policies.PasswordChanged)]
    public sealed class RoadmapsController : ApiBaseController
    {
        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPost("roadmap")]

        /// <summary>
        /// Creates a new roadmap within the current tenant.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/roadmaps/roadmap
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "Backend Developer Roadmap",
        ///       "description": "Complete backend learning path",
        ///       "path": "Backend",
        ///       "level": "Junior",
        ///       "publishedAt": "2026-02-01T00:00:00Z"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Roadmap created successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant is not found.</response>
        public async Task<ActionResult<RoadmapDto>> CreateRoadmap(CreateRoadmapRequest request)
        {
            var command = Mapper.Map<CreateRoadmap.Command>(request);

            return HandleResult(await Mediator.Send(command), "GetRoadmap", "Roadmaps");
        }

        /// <summary>
        /// Retrieves a roadmap by id (scoped to current tenant).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/roadmaps/roadmap/{roadmapId}
        ///
        /// <response code="200">Roadmap retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read roadmaps.</response>
        /// <response code="404">If the current tenant is not found or the roadmap does not exist.</response>

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet("roadmap/{roadmapId}")]
        public async Task<ActionResult<RoadmapDto>> GetRoadmap(string roadmapId)
        {
            var query = new GetRoadmap.Query { RoadmapId = roadmapId };

            return HandleResult(await Mediator.Send(query));
        }
        /// <summary>
        /// Retrieves roadmaps with filtering and cursor-based pagination.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/roadmaps?titleContains=Backend&pageSize=10
        ///
        /// Cursor pagination:
        ///
        ///     GET /api/roadmaps?cursor=2026-01-01T00:00:00Z&pageSize=10
        ///
        /// </remarks>
        /// <response code="200">Roadmaps retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read roadmaps.</response>
        /// <response code="404">If the current tenant is not found.</response>
        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet("public")]
        public async Task<ActionResult<PagedList<RoadmapDto, DateTime?>>> GetPublicRoadmaps([FromQuery] GetPublicRoadmapsRequest request)
        {
            var query = Mapper.Map<GetRoadmaps.Query>(request);
            query.IsPublished = true;
            query.IsVisible = true;
            return HandleResult(await Mediator.Send(query));
        }
        /// <summary>
        /// Retrieves all roadmaps (including unpublished) with filtering and cursor-based pagination.
        /// </summary>
        /// <remarks>
        /// Requires <c>Roadmaps_Manage</c> policy.
        /// </remarks>
        /// <response code="200">Roadmaps retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant is not found.</response>

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpGet("Manager")]
        public async Task<ActionResult<PagedList<RoadmapDto, DateTime?>>> GetManagerRoadmaps([FromQuery] GetManagerRoadmapsRequest request)
        {
            var query = Mapper.Map<GetRoadmaps.Query>(request);
            return HandleResult(await Mediator.Send(query));
        }

        /// <summary>
        /// Updates roadmap details.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/roadmaps/roadmap/{roadmapId}
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "Updated Title",
        ///       "description": "Updated description"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Roadmap updated successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant is not found or the roadmap does not exist.</response>
        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPatch("roadmap/{roadmapId}")]
        public async Task<ActionResult<RoadmapDto>> UpdateRoadmap(string roadmapId, UpdateRoadmapRequest request)
        {
            var command = Mapper.Map<UpdateRoadmapDetails.Command>(request);
            command.RoadmapId = roadmapId;

            return HandleResult(await Mediator.Send(command));
        }
        /// <summary>
        /// Publishes a roadmap.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/roadmaps/roadmap/{roadmapId}/publish
        ///
        /// </remarks>
        /// <response code="200">Roadmap published successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant is not found or the roadmap does not exist.</response>
        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPatch("roadmap/{roadmapId}/publish")]
        public async Task<ActionResult> PublishRoadmap(string roadmapId)
        {
            var command = new PublishRoadmap.Command { RoadmapId = roadmapId };
            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Toggles roadmap visibility.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/roadmaps/roadmap/{roadmapId}/visibility
        ///
        /// </remarks>
        /// <response code="200">Roadmap visibility updated successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant is not found or the roadmap does not exist.</response>        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPatch("roadmap/{roadmapId}/visibility")]
        public async Task<ActionResult> ToggleRoadmapVisibility(string roadmapId, ToggleRoadmapVisibleRequest request)
        {
            var command = new ToggleRoadmapVisible.Command
            {
                RoadmapId = roadmapId,
                IsVisible = request.IsVisible
            };

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Sets roadmap tags (replace all existing tags).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/roadmaps/roadmap/{roadmapId}/tags
        ///     {
        ///       "tagIds": ["id1", "id2"]
        ///     }
        ///
        /// </remarks>
        /// /// <response code="200">Tags updated successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage roadmaps.</response>
        /// <response code="404">If the current tenant, roadmap, or one of the tags does not exist.</response>
        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPut("roadmap/{roadmapId}/tags")]
        public async Task<ActionResult> SetTags(string roadmapId, SetTagsRequest request)
        {
            var command = new SetTags.Command
            {
                RoadmapId = roadmapId,
                TagIds = request.TagIds
            };

            return HandleResult(await Mediator.Send(command));
        }
        /// <summary>
        /// Adds tags to roadmap.
        /// </summary>
        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPost("roadmap/{roadmapId}/tags")]
        public async Task<ActionResult> AddTags(string roadmapId, AddTagsRequest request)
        {
            var command = new AddTags.Command
            {
                RoadmapId = roadmapId,
                TagIds = request.TagIds
            };

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Removes tags from roadmap.
        /// </summary>
        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPatch("roadmap/{roadmapId}/tags")]
        public async Task<ActionResult> RemoveTags(string roadmapId, RemoveTagsRequest request)
        {
            var command = new RemoveTags.Command
            {
                RoadmapId = roadmapId,
                TagIds = request.TagIds
            };

            return HandleResult(await Mediator.Send(command));
        }


    }
}
