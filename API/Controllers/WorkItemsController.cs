using API.Contracts.WorkItems;
using API.Contracts.WorkItems.Create;
using API.Contracts.WorkItems.Update;
using API.SetupProgram;
using Application.Common;
using Application.Features.WorkItems.Commands;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/work-items")]
[Authorize(Policy = Policies.PasswordChanged)]
[Authorize(Policy = Policies.Project_Access)]
public sealed class WorkItemsController : ApiBaseController
{
    /// <summary>
    /// Creates a new Epic within a specific project.
    /// </summary>
    /// <remarks>
    /// Creates an Epic work item.
    ///
    /// Epics represent high-level initiatives and contain only the common (parent) fields.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/epics
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "Authentication Module",
    ///       "description": "High-level authentication features",
    ///       "acceptanceCriteria": "All auth flows documented and reviewed",
    ///       "effortPoints": 8,
    ///       "priority": "High"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">The Epic creation payload.</param>
    /// <returns>The identifier of the created Epic.</returns>
    /// <response code="201">Epic created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_CreatePlanning)]
    [HttpPost("epics")]
    public async Task<ActionResult<string>> CreateEpic(string projectId, CreateEpicRequest request)
    {
        var command = Mapper.Map<CreateWorkItem.Command>(request);
        command.ProjectId = projectId;

        return HandleResult(await Mediator.Send(command), "GetWorkItem", "WorkItems");
    }

    /// <summary>
    /// Creates a new User Story within a specific project.
    /// </summary>
    /// <remarks>
    /// Creates a User Story work item.
    ///
    /// User Stories require storyPoints in addition to the common (parent) fields.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/user-stories
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "User can log in",
    ///       "description": "As a user, I want to log in using email and password",
    ///       "acceptanceCriteria": "Valid credentials allow access to dashboard",
    ///       "effortPoints": 5,
    ///       "priority": "Medium",
    ///       "storyPoints": 3
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">The User Story creation payload.</param>
    /// <returns>The identifier of the created User Story.</returns>
    /// <response code="201">User Story created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_CreatePlanning)]
    [HttpPost("user-stories")]
    public async Task<ActionResult<string>> CreateUserStory(string projectId, CreateUserStoryRequest request)
    {
        var command = Mapper.Map<CreateWorkItem.Command>(request);
        command.ProjectId = projectId;

        return HandleResult(await Mediator.Send(command), "GetWorkItem", "WorkItems");
    }

    /// <summary>
    /// Creates a new Task within a specific project.
    /// </summary>
    /// <remarks>
    /// Creates a Task work item.
    ///
    /// Tasks contain only the common (parent) fields.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/tasks
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "Refactor authentication service",
    ///       "description": "Improve code structure and naming",
    ///       "acceptanceCriteria": "Service passes unit tests and follows clean architecture",
    ///       "effortPoints": 3,
    ///       "priority": "Low"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">The Task creation payload.</param>
    /// <returns>The identifier of the created Task.</returns>
    /// <response code="201">Task created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_CreateExecution)]
    [HttpPost("tasks")]
    public async Task<ActionResult<string>> CreateTask(string projectId, CreateTaskRequest request)
    {
        var command = Mapper.Map<CreateWorkItem.Command>(request);
        command.ProjectId = projectId;

        return HandleResult(await Mediator.Send(command), "GetWorkItem", "WorkItems");
    }

    /// <summary>
    /// Creates a new Bug within a specific project.
    /// </summary>
    /// <remarks>
    /// Creates a Bug work item.
    ///
    /// Bugs require stepsToReproduce in addition to the common (parent) fields.
    /// Note: expectedResult and actualResult are typically updated later, but shown here as a comprehensive example.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/bugs
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "Login fails with expired token",
    ///       "description": "System throws 500 instead of returning 401",
    ///       "acceptanceCriteria": "Expired token must return 401 Unauthorized",
    ///       "effortPoints": 2,
    ///       "priority": "High",
    ///       "stepsToReproduce": "1. Login\n2. Wait for token expiry\n3. Call secured endpoint",
    ///       "expectedResult": "401 Unauthorized",
    ///       "actualResult": "500 Internal Server Error"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">The Bug creation payload.</param>
    /// <returns>The identifier of the created Bug.</returns>
    /// <response code="201">Bug created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_CreateExecution)]
    [HttpPost("bugs")]
    public async Task<ActionResult<string>> CreateBug(string projectId, CreateBugRequest request)
    {
        var command = Mapper.Map<CreateWorkItem.Command>(request);
        command.ProjectId = projectId;

        return HandleResult(await Mediator.Send(command), "GetWorkItem", "WorkItems");
    }

    /// <summary>
    /// Creates a new Test Case within a specific project.
    /// </summary>
    /// <remarks>
    /// Creates a Test Case work item.
    ///
    /// Test Cases require preconditions, steps, testEnvironment, and expectedResult in addition to the common (parent) fields.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/test-cases
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "Login with valid credentials",
    ///       "description": "Verify successful login scenario",
    ///       "acceptanceCriteria": "User is redirected to dashboard",
    ///       "effortPoints": 1,
    ///       "priority": "Medium",
    ///       "preconditions": "User account exists and is active",
    ///       "steps": "1. Open login page\n2. Enter valid email\n3. Enter valid password\n4. Click login",
    ///       "testEnvironment": "Software",
    ///       "expectedResult": "User is logged in successfully"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">The Test Case creation payload.</param>
    /// <returns>The identifier of the created Test Case.</returns>
    /// <response code="201">Test Case created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_CreateExecution)]
    [HttpPost("test-cases")]
    public async Task<ActionResult<string>> CreateTestCase(string projectId, CreateTestCaseRequest request)
    {
        var command = Mapper.Map<CreateWorkItem.Command>(request);
        command.ProjectId = projectId;

        return HandleResult(await Mediator.Send(command), "GetWorkItem", "WorkItems");
    }

    /// <summary>
    /// Retrieves a specific work item by its identifier.
    /// </summary>
    /// <remarks>
    /// Returns detailed information about a single work item within the specified project.
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{projectId}/work-items/{workItemId}
    ///
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="workItemId">Work item identifier.</param>
    /// <returns>The requested work item details.</returns>
    /// <response code="200">Returns the work item successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the work item does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_Read)]
    [HttpGet("{workItemId}")]
    public async Task<ActionResult<WorkItemDto>> GetWorkItem(string projectId, string workItemId)
    {
        var query = new GetWorkItem.Query
        {
            ProjectId = projectId,
            WorkItemId = workItemId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves a paginated list of work items for a specific project.
    /// </summary>
    /// <remarks>
    /// Returns work items in the specified project with filtering and cursor-based pagination.
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{projectId}/work-items?Type=Bug&Status=InProgress&Priority=High&PageSize=10&Cursor=2026-02-01T00:00:00Z
    ///
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="request">Filtering and pagination parameters.</param>
    /// <returns>A paginated list of work items.</returns>
    /// <response code="200">Returns the list of work items successfully.</response>
    /// <response code="400">If the query parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_Read)]
    [HttpGet]
    public async Task<ActionResult<PagedList<WorkItemDto, DateTime?>>> GetWorkItems(
        string projectId,
        [FromQuery] GetWorkItemsRequest request)
    {
        var parameters = Mapper.Map<WorkItemsSearchAndFilterParametersDto>(request);

        var query = new GetWorkItems.Query
        {
            ProjectId = projectId,
            Parameters = parameters
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Updates an Epic.
    /// </summary>
    /// <remarks>
    /// Partially updates an Epic work item within the specified project.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/projects/{projectId}/work-items/{workItemId}/epic
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "Authentication Module (updated)",
    ///       "description": "Updated epic description",
    ///       "acceptanceCriteria": "All auth flows documented and reviewed",
    ///       "effortPoints": 13,
    ///       "priority": "High",
    ///       "sprintId": "sprint-2026-01",
    ///       "status": "InProgress",
    ///       "isBlocked": false
    ///     }
    ///     
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="workItemId">Work item identifier.</param>
    /// <param name="request">Epic update payload.</param>
    /// <returns>The updated work item.</returns>
    /// <response code="200">Epic updated successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the work item does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdatePlanning)]
    [HttpPatch("{workItemId}/epic")]
    public async Task<ActionResult<WorkItemDto>> UpdateEpic(string projectId, string workItemId, UpdateEpicRequest request)
    {
        var command = Mapper.Map<UpdateWorkItem.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Updates a User Story.
    /// </summary>
    /// <remarks>
    /// Partially updates a User Story work item within the specified project.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/projects/{projectId}/work-items/{workItemId}/user-story
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "User can log in (updated)",
    ///       "description": "Updated story description",
    ///       "acceptanceCriteria": "Login works with valid credentials",
    ///       "effortPoints": 8,
    ///       "priority": "High",
    ///       "sprintId": "sprint-2026-01",
    ///       "status": "InTesting",
    ///       "isBlocked": false,
    ///       "storyPoints": 5
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="workItemId">Work item identifier.</param>
    /// <param name="request">User Story update payload.</param>
    /// <returns>The updated work item.</returns>
    [Authorize(Policy = Policies.WorkItems_UpdatePlanning)]
    [HttpPatch("{workItemId}/user-story")]
    public async Task<ActionResult<WorkItemDto>> UpdateUserStory(string projectId, string workItemId, UpdateUserStoryRequest request)
    {
        var command = Mapper.Map<UpdateWorkItem.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Updates a Task.
    /// </summary>
    /// <remarks>
    /// Partially updates a Task work item within the specified project.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/projects/{projectId}/work-items/{workItemId}/task
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "Refactor authentication service (updated)",
    ///       "description": "Updated task description",
    ///       "acceptanceCriteria": "Refactor completed and tests passing",
    ///       "effortPoints": 5,
    ///       "priority": "Low",
    ///       "sprintId": "sprint-2026-02",
    ///       "status": "InReview",
    ///       "isBlocked": true
    ///     }
    ///
    /// </remarks>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPatch("{workItemId}/task")]
    public async Task<ActionResult<WorkItemDto>> UpdateTask(string projectId, string workItemId, UpdateTaskRequest request)
    {
        var command = Mapper.Map<UpdateWorkItem.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Updates a Bug.
    /// </summary>
    /// <remarks>
    /// Partially updates a Bug work item within the specified project.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/projects/{projectId}/work-items/{workItemId}/bug
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "Login fails with expired token (updated)",
    ///       "description": "Updated bug details",
    ///       "acceptanceCriteria": "Return 401 on expired token",
    ///       "effortPoints": 3,
    ///       "priority": "High",
    ///       "sprintId": "sprint-2026-01",
    ///       "status": "InProgress",
    ///       "isBlocked": false,
    ///       "stepsToReproduce": "1. Login\n2. Wait token expiry\n3. Call API",
    ///       "expectedResult": "401 Unauthorized",
    ///       "actualResult": "500 Internal Server Error"
    ///     }
    ///
    /// </remarks>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPatch("{workItemId}/bug")]
    public async Task<ActionResult<WorkItemDto>> UpdateBug(string projectId, string workItemId, UpdateBugRequest request)
    {
        var command = Mapper.Map<UpdateWorkItem.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Updates a Test Case.
    /// </summary>
    /// <remarks>
    /// Partially updates a Test Case work item within the specified project.
    /// This example includes both definition fields and optional outcome fields (result/actualResult) if your request supports them.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/projects/{projectId}/work-items/{workItemId}/test-case
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "Login with valid credentials (updated)",
    ///       "description": "Updated test case description",
    ///       "acceptanceCriteria": "Login succeeds and dashboard opens",
    ///       "effortPoints": 2,
    ///       "priority": "Medium",
    ///       "sprintId": "sprint-2026-02",
    ///       "status": "InTesting",
    ///       "isBlocked": false,
    ///       "preconditions": "User exists and is active",
    ///       "steps": "1. Open login page\n2. Enter email\n3. Enter password\n4. Click login",
    ///       "testEnvironment": "Software",
    ///       "expectedResult": "User is redirected to dashboard",
    ///       "result": "Passed",
    ///       "actualResult": "Dashboard opened successfully"
    ///     }
    ///
    /// </remarks>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPatch("{workItemId}/test-case")]
    public async Task<ActionResult<WorkItemDto>> UpdateTestCase(string projectId, string workItemId, UpdateTestCaseRequest request)
    {
        var command = Mapper.Map<UpdateWorkItem.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deletes a work item permanently.
    /// </summary>
    /// <remarks>
    /// Permanently removes the work item from the specified project.
    ///
    /// Sample request:
    ///
    ///     DELETE /api/projects/{projectId}/work-items/{workItemId}
    ///
    /// </remarks>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="workItemId">Work item identifier.</param>
    /// <returns>No content if deletion succeeds.</returns>
    /// <response code="204">The work item was deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the work item does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>

    [Authorize(Policy = Policies.WorkItems_Delete)]
    [HttpDelete("{workItemId}")]
    public async Task<ActionResult> DeleteWorkItem(string projectId, string workItemId)
    {
        var command = new DeleteWorkItem.Command
        {
            ProjectId = projectId,
            WorkItemId = workItemId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Changes the workflow status of a work item.
    /// </summary>
    /// <remarks>
    /// Moves a work item between workflow columns (ToDo / ReadyForDev / InProgress / InReview / InTesting / ReadyForRelease / Done)
    /// using the domain transition rules.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/projects/{projectId}/work-items/{workItemId}/status
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "newStatus": "InProgress"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="workItemId">The work item identifier.</param>
    /// <param name="request">The new status payload.</param>
    /// <returns>The updated work item.</returns>
    /// <response code="200">Status changed successfully.</response>
    /// <response code="400">If the request is invalid or the transition is not allowed by workflow rules.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the work item does not exist in the project.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_ChangeStatus)]
    [HttpPatch("{workItemId}/status")]
    public async Task<ActionResult<WorkItemDto>> ChangeStatus(
        string projectId,
        string workItemId,
        ChangeWorkItemStatusRequest request)
    {
        var command = Mapper.Map<ChangeWorkItemStatus.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Updates the assignee of a work item.
    /// </summary>
    /// <remarks>
    /// Changes the assigned user of a work item. This endpoint allows for reassigning work items to different team members.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/projects/{projectId}/work-items/{workItemId}/assignee
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "assignedToUserId": "user-222"
    ///     }
    ///
    /// </remarks>
    /// 
    /// <param name="projectId">The project identifier.</param>
    /// <param name="workItemId">The work item identifier.</param>
    /// <param name="request">The new assignee payload.</param>
    /// <returns>The updated work item with the new assignee.</returns>
    /// <response code="200">Assignee updated successfully.</response>
    /// <response code="400">If the request is invalid or the user to assign does not exist.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project or permission to assign work items.</response>
    /// <response code="404">If the work item does not exist in the project.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPatch("{workItemId}/assignee")]
    public async Task<ActionResult<WorkItemDto>> UpdateAssignee(
        string projectId,
        string workItemId,
        UpdateWorkItemAssigneeRequest request)
    {
        var command = Mapper.Map<UpdateWorkItemAssignee.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }
}