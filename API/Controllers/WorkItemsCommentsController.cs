using API.Contracts.WorkItems.Comments;
using API.SetupProgram;
using Application.Common;
using Application.Features.WorkItems.Commands.Comments;
using Application.Features.WorkItems.DTOs.Comments;
using Application.Features.WorkItems.Queries.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/work-items")]
[Authorize(Policy = Policies.PasswordChanged)]
[Authorize(Policy = Policies.Project_Access)]
public sealed class WorkItemsCommentsController : ApiBaseController
{
    /// <summary>
    /// Creates a new WorkItem Comment.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/{workItemId}/comment
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "content": "This is a comment"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="request">The Comment creation payload.</param>
    /// <returns>No content.</returns>
    /// <response code="201">Comment created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPost("{workItemId}/comment")]
    public async Task<ActionResult<string>> AddWorkItemComment(
       string projectId,
       string workItemId,
       AddWorkItemCommentRequest request)
    {
        var command = Mapper.Map<AddWorkItemComment.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Get a specific WorkItem Comments.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{projectId}/work-items/{workItemId}/comments
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "pageSize": 5,
    ///       "cursor" : "date"
    ///     }
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="request">The Pagination Payload.</param>
    /// <returns>The list of the specified WorkItem comments.</returns>
    /// <response code="200">Comments retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_Read)]
    [HttpGet("{workItemId}/comments")]
    public async Task<ActionResult<PagedList<CommentDto, DateTime>>> GetWorkItemComments(
      string projectId,
      string workItemId,
      [FromQuery] GetWorkItemCommentsRequest request)
    {
        var query = Mapper.Map<GetWorkItemComments.Query>(request);
        query.ProjectId = projectId;
        query.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Delete a specific WorkItem CodeLinkes.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     DELETE /api/projects/{projectId}/work-items/{workItemId}/comment/{commentId}
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="commentId">The unique identifies of the comment to remove.</param>
    /// <returns>No content if the delete was successful.</returns>
    /// <response code="204">Delete was successful.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpDelete("{workItemId}/comment/{commentId}")]
    public async Task<IActionResult> DeleteWorkItemComment(
      string projectId,
      string workItemId,
      string commentId)
    {
        var command = new DeleteWorkItemComment.Command
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
            CommentId = commentId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Creates a new WorkItem Comment.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{projectId}/work-items/{workItemId}/comment/{commentId}
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "content": "This is the new comment"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="commentId">The unique identifies of the comment.</param>
    /// <param name="request">The Comment creation payload.</param>
    /// <returns>No content.</returns>
    /// <response code="201">Comment created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPatch("{workItemId}/comment/{commentId}")]
    public async Task<IActionResult> UpdateWorkItemComment(
       string projectId,
       string workItemId,
       string commentId,
       UpdateWorkItemCommentRequest request)
    {
        var command = Mapper.Map<UpdateWorkItemComment.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;
        command.CommentId = commentId;

        return HandleResult(await Mediator.Send(command));
    }
}