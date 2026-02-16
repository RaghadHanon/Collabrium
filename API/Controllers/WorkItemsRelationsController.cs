using API.Contracts.WorkItems.Relations;
using API.SetupProgram;
using Application.Features.WorkItems.Commands.Relations;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.Relations;
using Application.Features.WorkItems.Queries.Relations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/work-items")]
[Authorize(Policy = Policies.PasswordChanged)]
[Authorize(Policy = Policies.Project_Access)]
public sealed class WorkItemsRelationsController : ApiBaseController
{
    /// <summary>
    /// Creates a new WorkItem Relation.
    /// </summary>
    /// <remarks>
    /// Creates a new WorkItem Relation.
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/{workItemId}/relation
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "targetWorkItemId": "cbc412e6-befa-46d8-8d5b-79c72b3acc7f",
    ///       "relationType": "Parent"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="request">The Relation creation payload.</param>
    /// <returns>The dto of the created WorkItem relation.</returns>
    /// <response code="201">Relation created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPost("{workItemId}/relation")]
    public async Task<ActionResult<WorkItemDto>> AddWorkItemRelation(
       string projectId,
       string workItemId,
       AddWorkItemRelationRequest request)
    {
        var command = Mapper.Map<AddWorkItemRelation.Command>(request);
        command.ProjectId = projectId;
        command.SourceWorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Get a specific WorkItem Relations (optionally with a specific relation type).
    /// </summary>
    /// <remarks>
    /// Get a specific WorkItem Relations.
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{projectId}/work-items/{workItemId}/relations
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "relationType": "Parent"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="request">Filter on type payload.</param>
    /// <returns>The list of the specified WorkItem relations.</returns>
    /// <response code="200">Relations retrieved successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_Read)]
    [HttpGet("{workItemId}/relations")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemRelations(
      string projectId,
      string workItemId,
      [FromQuery] GetWorkItemRelationsRequest request)
    {
        var query = Mapper.Map<GetWorkItemRelations.Query>(request);
        query.ProjectId = projectId;
        query.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Delete a specific WorkItem Relations.
    /// </summary>
    /// <remarks>
    /// Delete a specific WorkItem Relations.
    ///
    /// Sample request:
    ///
    ///     DELETE /api/projects/{projectId}/work-items/{workItemId}/relations/{relationId}
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="relationId">The unique identifies of the relation to remove.</param>
    /// <returns>No content if the delete was successful.</returns>
    /// <response code="204">Delete was successful.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpDelete("{workItemId}/relation/{relationId}")]
    public async Task<ActionResult<WorkItemDto>> DeleteWorkItemRelation(
      string projectId,
      string workItemId,
      string relationId)
    {
        var command = new DeleteWorkItemRelation.Command{
            ProjectId = projectId,
            SourceWorkItemId = workItemId,
            RelationId = relationId 
        };

        return HandleResult(await Mediator.Send(command));
    }
}