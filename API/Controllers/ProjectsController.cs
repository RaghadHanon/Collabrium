using API.Contracts.ProjectHistory;
using API.Contracts.Projects;
using API.Contracts.WorkItems;
using API.SetupProgram;
using Application.Common;
using Application.Features.ProjectHistories.DTOs;
using Application.Features.Projects.Commands;
using Application.Features.Projects.DTOs;
using Application.Features.Projects.DTOs.Boards;
using Application.Features.Projects.Queries;
using Application.Features.ProjectsHistories.Queries;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/projects")]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class ProjectsController : ApiBaseController
{
    /// <summary>
    /// Creates a new project within the current tenant.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/projects/project
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "name": "Collabrium",
    ///       "description": "Project management platform",
    ///       "key": "COLLAB",
    ///       "teamId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "startDate": "2026-02-01T00:00:00Z",
    ///       "endDate": "2026-05-01T00:00:00Z"
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the created project.</response>
    /// <response code="400">If validation fails (duplicate name, invalid dates, etc.).</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If tenant/team not found.</response>
    [Authorize(Policy = Policies.Projects_Manage)]
    [HttpPost("project")]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectRequest request)
    {
        var command = Mapper.Map<CreateProject.Command>(request);

        return HandleResult(await Mediator.Send(command), "GetProject", "Projects");
    }

    /// <summary>
    /// Retrieves a project by id (scoped to current tenant).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/projects/project/{projectId}
    ///
    /// </remarks>
    /// <response code="200">Returns the project.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If project not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<ProjectDto>> GetProject(string projectId)
    {
        var query = new GetProject.Query { ProjectId = projectId };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves projects for the current tenant with filtering and cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/projects?nameContains=Collab&amp;pageSize=10
    ///
    /// Cursor pagination:
    ///
    ///     GET /api/projects?cursor=2026-01-01T00:00:00Z&amp;pageSize=10
    ///
    /// </remarks>
    /// <response code="200">Returns projects successfully.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [HttpGet]
    public async Task<ActionResult<PagedList<ProjectDto, DateTime?>>> GetProjects([FromQuery] GetProjectsRequest request)
    {
        var query = Mapper.Map<GetProjects.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Updates an existing project (name/description/status/dates/team assignment optional).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/projects/project/{projectId}
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "name": "Collabrium - Updated",
    ///       "description": "Updated description",
    ///       "status": "InProgress",
    ///       "startDate": "2026-02-01T00:00:00Z",
    ///       "endDate": "2026-06-01T00:00:00Z"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns the updated project.</response>
    /// <response code="400">If validation fails (duplicate name, invalid dates, inactive team, etc.).</response>
    /// <response code="404">If project (or team if provided) not found.</response>
    [Authorize(Policy = Policies.Projects_Manage)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpPatch("project/{projectId}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(string projectId, UpdateProjectRequest request)
    {
        var command = Mapper.Map<UpdateProject.Command>(request);
        command.ProjectId = projectId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deletes a project.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/projects/project/{projectId}
    ///
    /// </remarks>
    /// <response code="204">Deleted successfully.</response>
    /// <response code="404">If project not found.</response>
    [Authorize(Policy = Policies.Projects_Manage)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpDelete("project/{projectId}")]
    public async Task<ActionResult> DeleteProject(string projectId)
    {
        var command = new DeleteProject.Command { ProjectId = projectId };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Assigns an existing project to a team.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/projects/project/{projectId}/assign-team
    ///     Content-RelationType: application/json
    ///
    ///     { "teamId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
    ///
    /// </remarks>
    /// <response code="200">Returns updated project.</response>
    /// <response code="400">If team is inactive.</response>
    /// <response code="404">If project/team not found.</response>
    [Authorize(Policy = Policies.Projects_Manage)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpPatch("project/{projectId}/assign-team")]
    public async Task<ActionResult<ProjectDto>> AssignProjectToTeam(string projectId, AssignProjectToTeamRequest request)
    {
        var command = new AssignProjectToTeam.Command
        {
            ProjectId = projectId,
            TeamId = request.TeamId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Unassigns a project from its team (sets TeamId to null).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/projects/project/{projectId}/unassign-team
    ///
    /// </remarks>
    /// <response code="200">Returns updated project.</response>
    /// <response code="404">If project not found.</response>
    [Authorize(Policy = Policies.Projects_Manage)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpPatch("project/{projectId}/unassign-team")]
    public async Task<ActionResult<ProjectDto>> UnassignProjectFromTeam(string projectId)
    {
        var command = new UnassignProjectFromTeam.Command { ProjectId = projectId };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Retrieves backlog for a project.
    /// ToDo = all work items in the project that are not assigned to any sprint.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/projects/project/{projectId}/backlog
    ///
    /// </remarks>
    /// <response code="200">Returns backlog items.</response>
    /// <response code="404">If project not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("project/{projectId}/backlog")]
    public async Task<ActionResult<IReadOnlyList<WorkItemDto>>> GetProjectBacklog(string projectId,[FromQuery] GetProjectBacklogRequest request)
    {
        var query = Mapper.Map<GetProjectBacklog.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the project board (workflow columns) with work items grouped by status.
    /// </summary>
    /// <remarks>
    /// Returns the board columns:
    /// ToDo / ReadyForDev / InProgress / InReview / InTesting / ReadyForRelease / Done
    /// with the items under each column.
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{projectId}/board
    ///
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="request">Get Project Board payload</param>
    /// <returns>The board data grouped by workflow status.</returns>
    /// <response code="200">Board loaded successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If tenant/project not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("project/{projectId}/board")]
    public async Task<ActionResult<BoardDto>> GetProjectBoard(string projectId, [FromQuery] GetProjectBoardRequest request)
    {
        var query = Mapper.Map<GetProjectBoard.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the projects list of user stories.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{projectId}/user-stories
    ///
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <returns>The list of user stories of a project.</returns>
    /// <response code="200">User stories loaded successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If tenant/project not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("project/{projectId}/user-stories")]
    public async Task<ActionResult<BoardDto>> GetProjectUserStories(string projectId)
    {
        var query = new GetWorkItemsByType.Query
        {
            ProjectId = projectId,
            Type = WorkItemType.UserStory
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the projects list of Epics.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{projectId}/epics
    ///
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <returns>The list of epics of a project.</returns>
    /// <response code="200">Epics loaded successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If tenant/project not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("project/{projectId}/epics")]
    public async Task<ActionResult<BoardDto>> GetProjectEpics(string projectId)
    {
        var query = new GetWorkItemsByType.Query
        {
            ProjectId = projectId,
            Type = WorkItemType.Epic
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the projects list of history actions.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{projectId}/project-history
    ///
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="request">Filtering and pagination parameters.</param>
    /// <returns>The list of project history actions of a project.</returns>
    /// <response code="200">Project history loaded successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If tenant/project not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("{projectId}/project-history")]
    public async Task<ActionResult<PagedList<ProjectHistoryDto, DateTime?>>> GetProjectHistory(
        string projectId,
        [FromQuery] GetProjectHistoryRequest request)
    {
        var query = Mapper.Map<GetProjectHistory.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }
}