using API.Contracts.Sprints;
using API.SetupProgram;
using Application.Common;
using Application.Features.Sprints.Commands;
using Application.Features.Sprints.DTOs;
using Application.Features.Sprints.Queries;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/projects/project/{projectId}/sprints")]
[Authorize(Policy = Policies.PasswordChanged)]
[Authorize(Policy = Policies.Project_Access)]
public sealed class SprintsController : ApiBaseController
{
    /// <summary>
    /// Creates a new sprint within the current tenant (scoped to a project).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/projects/project/{ProjectId}/sprints/sprint
    ///
    /// </remarks>
    /// <param name="projectId">The project id to which the sprint belongs (from route).</param>
    /// <returns>The created sprint.</returns>
    /// <response code="201">Returns the created sprint.</response>
    /// <response code="400">If validation fails (end before start, etc.).</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If tenant or project not found.</response>
    [Authorize(Policy = Policies.Sprints_Manage)]
    [HttpPost("sprint")]
    public async Task<ActionResult<SprintDto>> CreateSprint(string projectId)
    {
        var command = new CreateSprint.Command()
        {
            ProjectId = projectId
        };

        return HandleResult(await Mediator.Send(command), "GetSprint", "Sprints");
    }

    /// <summary>
    /// Retrieves a sprint by id (scoped to current tenant).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/projects/project/{ProjectId}/sprints/sprint/{sprintId}
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <param name="projectId">The project id to which the sprint belongs (from route).</param>
    /// <returns>The sprint details.</returns>
    /// <response code="200">Returns the sprint.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.Sprints_Read)]
    [HttpGet("sprint/{sprintId}")]
    public async Task<ActionResult<SprintDto>> GetSprint(string projectId, string sprintId)
    {
        var query = new GetSprint.Query
        {
            SprintId = sprintId,
            ProjectId = projectId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves sprints for a project with filtering and cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/projects/project/{ProjectId}/sprints?amp;name=COL Sprint 1&amp;status=Active&amp;pageSize=10
    ///
    /// Cursor pagination:
    ///
    ///     GET /api/projects/project/{ProjectId}/sprints?amp;cursor=2026-02-01T00:00:00Z&amp;pageSize=10
    ///
    /// </remarks>
    /// <param name="request">Filter and pagination parameters.</param>
    /// <param name="projectId">The project id to which the sprints belongs (from route).</param>
    /// <returns>A paged list of sprints.</returns>
    /// <response code="200">Returns sprints successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [Authorize(Policy = Policies.Sprints_Read)]
    [HttpGet]
    public async Task<ActionResult<PagedList<SprintDto, DateTime?>>> GetSprints(string projectId, [FromQuery] GetSprintsRequest request)
    {
        var query = Mapper.Map<GetSprints.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Updates an existing sprint (name/date range).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/projects/project/{ProjectId}/sprints/sprint/{sprintId}
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "name": "Sprint 1",
    ///       "goal": "Complete user authentication module",
    ///       "startDate": "2026-02-02T00:00:00Z",
    ///       "endDate": "2026-02-15T23:59:59Z"
    ///     }
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <param name="projectId">The project id to which the sprint belongs (from route).</param>
    /// <param name="request">Update payload.</param>
    /// <returns>The updated sprint.</returns>
    /// <response code="200">Returns the updated sprint.</response>
    /// <response code="400">If validation fails.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.Sprints_Manage)]
    [HttpPatch("sprint/{sprintId}")]
    public async Task<ActionResult<SprintDto>> UpdateSprint(string projectId, string sprintId, UpdateSprintRequest request)
    {
        var command = Mapper.Map<UpdateSprint.Command>(request);
        command.SprintId = sprintId;
        command.ProjectId = projectId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deletes a sprint (only if it has no work items).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/projects/project/{ProjectId}/sprints/sprint/{sprintId}
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <param name="projectId">The project id to which the sprint belongs (from route).</param>
    /// <response code="204">Deleted successfully.</response>
    /// <response code="400">If sprint has work items.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.Sprints_Manage)]
    [HttpDelete("sprint/{sprintId}")]
    public async Task<ActionResult> DeleteSprint(string projectId, string sprintId)
    {
        var command = new DeleteSprint.Command
        {
            SprintId = sprintId,
            ProjectId = projectId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Starts a sprint.
    /// Rule: a project can have only one active sprint at a time.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/projects/project/{ProjectId}/sprints/sprint/{sprintId}/start
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "name": "Sprint 1",
    ///       "goal": "Complete user authentication module",
    ///       "startDate": "2026-02-01T00:00:00Z",
    ///       "endDate": "2026-02-14T23:59:59Z"
    ///     }
    ///     
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <param name="projectId">The project id to which the sprint belongs (from route).</param>
    /// <param name="request">Sprint schedule details.</param>
    /// <returns>The updated sprint.</returns>
    /// <response code="200">Returns started sprint.</response>
    /// <response code="400">If another active sprint already exists in the project.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.Sprints_Manage)]
    [HttpPatch("sprint/{sprintId}/start")]
    public async Task<ActionResult<SprintDto>> StartSprint(string projectId, string sprintId, StartSprintRequest request)
    {
        var command = Mapper.Map<StartSprint.Command>(request);
        command.SprintId = sprintId;
        command.ProjectId = projectId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Cancle a sprint.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/projects/project/{ProjectId}/sprints/sprint/{sprintId}/cancel
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <param name="projectId">The project id to which the sprint belongs (from route).</param>
    /// <returns>The updated sprint.</returns>
    /// <response code="200">Returns cancel sprint.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.Sprints_Manage)]
    [HttpPatch("sprint/{sprintId}/cancel")]
    public async Task<ActionResult<SprintDto>> CancelSprint(string projectId, string sprintId)
    {
        var command = new CancelSprint.Command
        {
            SprintId = sprintId,
            ProjectId = projectId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Completes an active sprint.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/projects/project/{ProjectId}/sprints/sprint/{sprintId}/complete
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <param name="projectId">The project id to which the sprint belongs (from route).</param>
    /// <returns>The updated sprint.</returns>
    /// <response code="200">Returns completed sprint.</response>
    /// <response code="400">If sprint is not active.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.Sprints_Manage)]
    [HttpPatch("sprint/{sprintId}/complete")]
    public async Task<ActionResult<SprintDto>> CompleteSprint(string projectId, string sprintId)
    {
        var command = new CompleteSprint.Command
        {
            SprintId = sprintId,
            ProjectId = projectId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Retrieves the list of user stories for a sprint.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{ProjectId}/sprints/{sprintId}/user-stories
    ///
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="sprintId">Sprint identifier.</param>
    /// <returns>The list of user stories of a sprint.</returns>
    /// <response code="200">User stories loaded successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If tenant/project/sprint not found.</response>
    [Authorize(Policy = Policies.Sprints_Read)]
    [HttpGet("sprint/{sprintId}/user-stories")]
    public async Task<ActionResult<WorkItemListItemDto>> GetSprintUserStories(string projectId, string sprintId)
    {
        var query = new GetWorkItemsByType.Query
        {
            ProjectId = projectId,
            SprintId = sprintId,
            Type = WorkItemType.UserStory
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Add list of WorkItems to sprint.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{ProjectId}/sprints/sprint/{sprintId}/work-items
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "workItemIds": [
    ///         "11111111-1111-1111-1111-111111111111",
    ///         "22222222-2222-2222-2222-222222222222"
    ///       ]
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="sprintId">Sprint identifier.</param>
    /// <param name="request">List of work item ids to be added to sprint.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">WorkItems added successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If tenant/project/sprint not found.</response>
    [Authorize(Policy = Policies.Sprints_Manage)]
    [HttpPost("sprint/{sprintId}/work-items")]
    public async Task<ActionResult<WorkItemListItemDto>> AddWorkItemsToSprint(string projectId, string sprintId, AddWorkItemsToSprintRequest request)
    {
       var command = Mapper.Map<AddWorkItemsToSprint.Command>(request);
        command.ProjectId = projectId;
        command.SprintId = sprintId;

        return HandleResult(await Mediator.Send(command));
    }
}