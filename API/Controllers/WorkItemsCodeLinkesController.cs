using API.Contracts.WorkItems.CodeLinkes;
using API.Contracts.WorkItems.Comments;
using API.SetupProgram;
using Application.Features.WorkItems.Commands.CodeLinks;
using Application.Features.WorkItems.Commands.Comments;
using Application.Features.WorkItems.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/work-items")]
[Authorize(Policy = Policies.PasswordChanged)]
[Authorize(Policy = Policies.Project_Access)]
public sealed class WorkItemsCodeLinkesController : ApiBaseController
{
    /// <summary>
    /// Creates a new WorkItem CodeLink.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{ProjectId}/work-items/{workItemId}/codeLink
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "url": "https:\\github.link",
    ///       "linkType": "Branch",
    ///       "displayName": "Fix Auth Bug"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="request">The CodeLink creation payload.</param>
    /// <returns>No content.</returns>
    /// <response code="201">CodeLink created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPost("{workItemId}/code-link")]
    public async Task<ActionResult<string>> AddWorkItemCodeLink(
       string projectId,
       string workItemId,
       AddWorkItemCodeLinkRequest request)
    {
        var command = Mapper.Map<AddWorkItemCodeLink.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Delete a specific WorkItem CodeLink.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     DELETE /api/projects/{ProjectId}/work-items/{workItemId}/code-links/{AttachmentId}
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="codeLinkId">The unique identifies of the codeLink to remove.</param>
    /// <returns>No content if the delete was successful.</returns>
    /// <response code="204">Delete was successful.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpDelete("{workItemId}/code-link/{codeLinkId}")]
    public async Task<IActionResult> DeleteWorkItemCodeLink(
      string projectId,
      string workItemId,
      string codeLinkId)
    {
        var command = new DeleteWorkItemCodeLink.Command
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
            CodeLinkId = codeLinkId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Creates a new WorkItem CodeLink.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{ProjectId}/work-items/{workItemId}/code-links/{AttachmentId}
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "url": "https:\\github.link",
    ///       "linkType": "Branch",
    ///       "displayName": "Fix Auth Bug"
    ///     }
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="codeLinkId">The unique identifies of the codeLink.</param>
    /// <param name="request">The CodeLink creation payload.</param>
    /// <returns>No content.</returns>
    /// <response code="201">CodeLink created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPatch("{workItemId}/code-link/{codeLinkId}")]
    public async Task<ActionResult<WorkItemDto>> UpdateWorkItemCodeLink(
       string projectId,
       string workItemId,
       string codeLinkId,
       UpdateWorkItemCodeLinkRequest request)
    {
        var command = Mapper.Map<UpdateWorkItemCodeLink.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;
        command.CodeLinkId = codeLinkId;

        return HandleResult(await Mediator.Send(command));
    }
}