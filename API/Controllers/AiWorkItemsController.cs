using API.Contracts.AI.WorkItems;
using API.SetupProgram;
using Application.Features.AI.DTOs.WorkItems;
using Application.Features.AI.Queries.WorkItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/projects/{projectId}/work-items/ai-suggestions")]
[Authorize(Policy = Policies.PasswordChanged)]
[Authorize(Policy = Policies.Project_Access)]
public sealed class AiWorkItemsController : ApiBaseController
{
    /// <summary>
    /// Generates AI-suggested fields for creating an Epic work item.
    /// </summary>
    /// <remarks>
    /// Generates suggested Epic creation fields based on a short user-provided summary.
    ///
    /// The returned fields can be reviewed and edited by the user before calling the normal Epic creation endpoint.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/ai-suggestions/epics
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "summary": "Build authentication module with login, logout, and password reset"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">The short summary used to generate suggested Epic fields.</param>
    /// <returns>Suggested fields for creating an Epic work item.</returns>
    /// <response code="200">Returns the generated Epic field suggestions.</response>
    /// <response code="400">If the summary is empty or invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project or permission to create planning work items.</response>
    /// <response code="500">If an unexpected error occurred while generating the suggestion.</response>
    [Authorize(Policy = Policies.WorkItems_CreatePlanning)]
    [HttpPost("epics")]
    public async Task<ActionResult<EpicFieldsSuggestionDto>> GenerateEpicFieldsSuggestion(
        string projectId,
        [FromBody] GenerateWorkItemFieldsSuggestionRequest request)
    {
        var query = Mapper.Map<GenerateEpicFieldsSuggestion.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates AI-suggested fields for creating a User Story work item.
    /// </summary>
    /// <remarks>
    /// Generates suggested User Story creation fields based on a short user-provided summary.
    ///
    /// The response includes the common work item fields plus story points.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/ai-suggestions/user-stories
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "summary": "As a user, I want to log in with email and password so I can access my dashboard"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">The short summary used to generate suggested User Story fields.</param>
    /// <returns>Suggested fields for creating a User Story work item.</returns>
    /// <response code="200">Returns the generated User Story field suggestions.</response>
    /// <response code="400">If the summary is empty or invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project or permission to create planning work items.</response>
    /// <response code="500">If an unexpected error occurred while generating the suggestion.</response>
    [Authorize(Policy = Policies.WorkItems_CreatePlanning)]
    [HttpPost("user-stories")]
    public async Task<ActionResult<UserStoryFieldsSuggestionDto>> GenerateUserStoryFieldsSuggestion(
        string projectId,
        [FromBody] GenerateWorkItemFieldsSuggestionRequest request)
    {
        var query = Mapper.Map<GenerateUserStoryFieldsSuggestion.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates AI-suggested fields for creating a Task work item.
    /// </summary>
    /// <remarks>
    /// Generates suggested Task creation fields based on a short user-provided summary.
    ///
    /// The returned fields can be reviewed and edited by the user before calling the normal Task creation endpoint.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/ai-suggestions/tasks
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "summary": "Refactor JWT token validation and make expired tokens return 401"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">The short summary used to generate suggested Task fields.</param>
    /// <returns>Suggested fields for creating a Task work item.</returns>
    /// <response code="200">Returns the generated Task field suggestions.</response>
    /// <response code="400">If the summary is empty or invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project or permission to create execution work items.</response>
    /// <response code="500">If an unexpected error occurred while generating the suggestion.</response>
    [Authorize(Policy = Policies.WorkItems_CreateExecution)]
    [HttpPost("tasks")]
    public async Task<ActionResult<TaskFieldsSuggestionDto>> GenerateTaskFieldsSuggestion(
        string projectId,
        [FromBody] GenerateWorkItemFieldsSuggestionRequest request)
    {
        var query = Mapper.Map<GenerateTaskFieldsSuggestion.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates AI-suggested fields for creating a Bug work item.
    /// </summary>
    /// <remarks>
    /// Generates suggested Bug creation fields based on a short user-provided summary.
    ///
    /// The response includes the common work item fields plus steps to reproduce, expected result, and actual result.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/ai-suggestions/bugs
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "summary": "After the access token expires, the dashboard keeps loading instead of redirecting to login"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">The short summary used to generate suggested Bug fields.</param>
    /// <returns>Suggested fields for creating a Bug work item.</returns>
    /// <response code="200">Returns the generated Bug field suggestions.</response>
    /// <response code="400">If the summary is empty or invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project or permission to create execution work items.</response>
    /// <response code="500">If an unexpected error occurred while generating the suggestion.</response>
    [Authorize(Policy = Policies.WorkItems_CreateExecution)]
    [HttpPost("bugs")]
    public async Task<ActionResult<BugFieldsSuggestionDto>> GenerateBugFieldsSuggestion(
        string projectId,
        [FromBody] GenerateWorkItemFieldsSuggestionRequest request)
    {
        var query = Mapper.Map<GenerateBugFieldsSuggestion.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates AI-suggested fields for creating a Test Case work item.
    /// </summary>
    /// <remarks>
    /// Generates suggested Test Case creation fields based on a short user-provided summary.
    ///
    /// The response includes the common work item fields plus preconditions, test steps, test environment, and expected result.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/ai-suggestions/test-cases
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "summary": "Verify that a user can log in successfully using valid email and password"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">The short summary used to generate suggested Test Case fields.</param>
    /// <returns>Suggested fields for creating a Test Case work item.</returns>
    /// <response code="200">Returns the generated Test Case field suggestions.</response>
    /// <response code="400">If the summary is empty or invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project or permission to create execution work items.</response>
    /// <response code="500">If an unexpected error occurred while generating the suggestion.</response>
    [Authorize(Policy = Policies.WorkItems_CreateExecution)]
    [HttpPost("test-cases")]
    public async Task<ActionResult<TestCaseFieldsSuggestionDto>> GenerateTestCaseFieldsSuggestion(
        string projectId,
        [FromBody] GenerateWorkItemFieldsSuggestionRequest request)
    {
        var query = Mapper.Map<GenerateTestCaseFieldsSuggestion.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }
}