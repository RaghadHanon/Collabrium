using API.Common.Attributes;
using API.Contracts.Tenants;
using API.Contracts.WorkItems.Attachments;
using API.SetupProgram;
using Application.Features.WorkItems.Commands.Attachments;
using Application.Features.WorkItems.DTOs.Attachment;
using Application.Features.WorkItems.Queries.Attachments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/projects/{ProjectId}/work-items")]
[Authorize(Policy = Policies.PasswordChanged)]
[Authorize(Policy = Policies.Project_Access)]
public sealed class WorkItemsAttachmentsController : ApiBaseController
{
    /// <summary>
    /// Creates a new WorkItem Attachment.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     POST /api/projects/{ProjectId}/work-items/{workItemId}/attachment
    ///     Content-RelationType: multipart/form-data
    ///     
    ///     Form Data:
    ///     key: "file", value: [Select file], type: File
    ///     key: "type", value: "Video", type: Text
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="request">The Attachment creation payload.</param>
    /// <returns>No content.</returns>
    /// <response code="201">Attachment created successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [RejectUnknownFormFields(typeof(AddWorkItemAttachmentRequest), allowEmptyRequest: false)]
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPost("{workItemId}/attachment")]
    public async Task<ActionResult<string>> AddWorkItemAttachment(
       string projectId,
       string workItemId,
       [FromForm] AddWorkItemAttachmentRequest request)
    {
        var command = Mapper.Map<AddWorkItemAttachment.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Get a specific WorkItem Attachments.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     GET /api/projects/{ProjectId}/work-items/{workItemId}/attachments
    ///     
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <returns>The list of the specified WorkItem Attachments.</returns>
    /// <response code="200">Attachment retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_Read)]
    [HttpGet("{workItemId}/attachments")]
    public async Task<ActionResult<List<AttachmentDto>>> GetWorkItemAttachments(
      string projectId,
      string workItemId)
    {
        var query = new GetWorkItemAttachments.Query
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Delete a specific WorkItem Attachments.
    /// </summary>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     DELETE /api/projects/{ProjectId}/work-items/{workItemId}/attachment/{attachmentId}
    ///
    /// </remarks>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="workItemId">The unique identifies of the workItem.</param>
    /// <param name="attachmentId">The unique identifies of the attachment to remove.</param>
    /// <returns>No content if the delete was successful.</returns>
    /// <response code="204">Delete was successful.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    /// <response code="500">If an unexpected error occurred.</response>
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpDelete("{workItemId}/attachment/{attachmentId}")]
    public async Task<IActionResult> DeleteWorkItemAttachment(
      string projectId,
      string workItemId,
      string attachmentId)
    {
        var command = new DeleteWorkItemAttachment.Command
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
            AttachmentId = attachmentId
        };

        return HandleResult(await Mediator.Send(command));
    }
}