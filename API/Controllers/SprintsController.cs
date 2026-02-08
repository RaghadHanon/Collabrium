using API.Contracts.Sprints;
using API.SetupProgram;
using Application.Common;
using Application.Features.Sprints.Commands;
using Application.Features.Sprints.DTOs;
using Application.Features.Sprints.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/sprints")]
[Authorize(Policy = Policies.PasswordChanged)]
[Authorize(Policy = Policies.MustHaveProjectAccess)]
public sealed class SprintsController : ApiBaseController
{
    /// <summary>
    /// Creates a new sprint within the current tenant (scoped to a project).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/sprints/sprint
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "name": "Sprint 1",
    ///       "goal": "Complete initial features",
    ///       "startDate": "2026-02-01T00:00:00Z",
    ///       "endDate": "2026-02-14T23:59:59Z"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">Sprint creation details.</param>
    /// <returns>The created sprint.</returns>
    /// <response code="201">Returns the created sprint.</response>
    /// <response code="400">If validation fails (end before start, etc.).</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If tenant or project not found.</response>
    [Authorize(Policy = Policies.ProductOwner)]
    [HttpPost("sprint")]
    public async Task<ActionResult<SprintDto>> CreateSprint(CreateSprintRequest request)
    {
        var command = Mapper.Map<CreateSprint.Command>(request);

        return HandleResult(await Mediator.Send(command), "GetSprint", "Sprints");
    }

    /// <summary>
    /// Retrieves a sprint by id (scoped to current tenant).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/sprints/sprint/{sprintId}
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <returns>The sprint details.</returns>
    /// <response code="200">Returns the sprint.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.TenantMember)]
    [HttpGet("sprint/{sprintId}")]
    public async Task<ActionResult<SprintDto>> GetSprint(string sprintId)
    {
        var query = new GetSprint.Query { SprintId = sprintId };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves sprints for a project with filtering and cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/sprints?projectId=...&amp;nameContains=Sprint&amp;status=Active&amp;pageSize=10
    ///
    /// Cursor pagination:
    ///
    ///     GET /api/sprints?projectId=...&amp;cursor=2026-02-01T00:00:00Z&amp;pageSize=10
    ///
    /// </remarks>
    /// <param name="request">Filter and pagination parameters.</param>
    /// <returns>A paged list of sprints.</returns>
    /// <response code="200">Returns sprints successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [Authorize(Policy = Policies.TenantMember)]
    [HttpGet]
    public async Task<ActionResult<PagedList<SprintDto, DateTime?>>> GetSprints([FromQuery] GetSprintsRequest request)
    {
        var query = Mapper.Map<GetSprints.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Updates an existing sprint (name/date range).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/sprints/sprint/{sprintId}
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "name": "Sprint 1 - Updated",
    ///       "goal": "Updated goal",
    ///       "startDate": "2026-02-02T00:00:00Z",
    ///       "endDate": "2026-02-15T23:59:59Z"
    ///     }
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <param name="request">Update payload.</param>
    /// <returns>The updated sprint.</returns>
    /// <response code="200">Returns the updated sprint.</response>
    /// <response code="400">If validation fails.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.ProductTeamManager)]
    [HttpPatch("sprint/{sprintId}")]
    public async Task<ActionResult<SprintDto>> UpdateSprint(string sprintId, UpdateSprintRequest request)
    {
        var command = Mapper.Map<UpdateSprint.Command>(request);
        command.SprintId = sprintId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deletes a sprint (only if it has no work items).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/sprints/sprint/{sprintId}
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <response code="204">Deleted successfully.</response>
    /// <response code="400">If sprint has work items.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.ProductOwner)]
    [HttpDelete("sprint/{sprintId}")]
    public async Task<ActionResult> DeleteSprint(string sprintId)
    {
        var command = new DeleteSprint.Command { SprintId = sprintId };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Starts a sprint.
    /// Rule: a project can have only one active sprint at a time.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/sprints/sprint/{sprintId}/start
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <returns>The updated sprint.</returns>
    /// <response code="200">Returns started sprint.</response>
    /// <response code="400">If another active sprint already exists in the project.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.ProductOwner)]
    [HttpPatch("sprint/{sprintId}/start")]
    public async Task<ActionResult<SprintDto>> StartSprint(string sprintId)
    {
        var command = new StartSprint.Command { SprintId = sprintId };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Cancle a sprint.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/sprints/sprint/{sprintId}/cancel
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <returns>The updated sprint.</returns>
    /// <response code="200">Returns cancel sprint.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.ProductOwner)]
    [HttpPatch("sprint/{sprintId}/cancel")]
    public async Task<ActionResult<SprintDto>> CancelSprint(string sprintId)
    {
        var command = new CancelSprint.Command { SprintId = sprintId };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Completes an active sprint.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/sprints/sprint/{sprintId}/complete
    ///
    /// </remarks>
    /// <param name="sprintId">The sprint id.</param>
    /// <returns>The updated sprint.</returns>
    /// <response code="200">Returns completed sprint.</response>
    /// <response code="400">If sprint is not active.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If sprint not found.</response>
    [Authorize(Policy = Policies.ProductOwner)]
    [HttpPatch("sprint/{sprintId}/complete")]
    public async Task<ActionResult<SprintDto>> CompleteSprint(string sprintId)
    {
        var command = new CompleteSprint.Command { SprintId = sprintId };

        return HandleResult(await Mediator.Send(command));
    }
}