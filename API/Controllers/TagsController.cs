using API.Contracts.Tags;
using API.SetupProgram;
using Application.Common;
using Application.Features.Tags.Commands;
using Application.Features.Tags.DTOs;
using Application.Features.Tags.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/tags")]
[Authorize(Policy = Policies.PasswordChanged)]
public class TagsController : ApiBaseController
{
    /// <summary>
    /// Creates a new tag within the current tenant.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/tags/tag
    ///     {
    ///       "name": "Backend"
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Tag created successfully and returned in the response body.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to manage tags.</response>
    /// <response code="404">If the current tenant is not found.</response>
    /// <response code="409">If a tag with the same name already exists in the tenant.</response>
    /// <response code="500">If the tag was created but could not be loaded due to an unexpected server error.</response>
    [Authorize(Policy = Policies.Tags_Manage)]
    [HttpPost("tag")]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagRequest request)
    {
        var command = Mapper.Map<CreateTag.Command>(request);

        return HandleResult(await Mediator.Send(command), "GetTag", "Tags");
    }
    /// <summary>
    /// Retrieves a tag by id.
    /// </summary>
    /// <remarks>
    ///     GET /api/tags/tag/{tagId}
    /// </remarks>
    /// <response code="200">Tag retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read tags.</response>
    /// <response code="404">If the tenant is not found or the tag does not exist.</response>
    [Authorize(Policy = Policies.Tags_Read)]
    [HttpGet("tag/{tagId}")]
    public async Task<ActionResult<TagDto>> GetTag(string tagId)
    {
        var query = new GetTag.Query
        {
            TagId = tagId
        };

        return HandleResult(await Mediator.Send(query));
    }
    /// <summary>
    /// Retrieves a paginated list of tags within the current tenant,
    /// with optional filtering and cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// Only users with the <c>Tags_Read</c> policy can access this endpoint.
    ///
    /// Filtering example:
    ///
    ///     GET /api/tags?nameContains=Back&pageSize=10
    ///
    /// Cursor-based pagination example:
    ///
    ///     GET /api/tags?cursor=2026-01-01T00:00:00Z&pageSize=10
    ///
    /// The cursor represents the last item's sorting value (e.g., CreatedAt).
    /// </remarks>
    /// <response code="200">Tags retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read tags.</response>
    /// <response code="404">If the current tenant is not found.</response>
    [Authorize(Policy = Policies.Tags_Read)]
    [HttpGet]
    public async Task<ActionResult<PagedList<TagDto, DateTime?>>> GetTags([FromQuery] GetTagsRequest request)
    {
        var query = Mapper.Map<GetTags.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Updates the name of an existing tag within the current tenant.
    /// </summary>
    /// <remarks>
    /// Only users with the <c>Tags_Manage</c> policy can update tags.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/tags/tag/{tagId}
    ///     Content-Type: application/json
    ///     {
    ///         "name": "Updated Name"
    ///     }
    ///
    /// The tag name must remain unique within the tenant.
    /// </remarks>
    /// <response code="200">Tag updated successfully and returned in the response body.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to manage tags.</response>
    /// <response code="404">If the current tenant is not found or the tag does not exist.</response>
    /// <response code="409">If another tag with the same name already exists.</response>
    [Authorize(Policy = Policies.Tags_Manage)]
    [HttpPatch("tag/{tagId}")]
    public async Task<ActionResult<TagDto>> UpdateTag(string tagId, UpdateTagRequest request)
    {
        var command = Mapper.Map<UpdateTag.Command>(request);
        command.TagId = tagId;

        return HandleResult(await Mediator.Send(command));
    }
    /// <summary>
    /// Deletes an existing tag within the current tenant.
    /// </summary>
    /// <remarks>
    /// Only users with the <c>Tags_Manage</c> policy can delete tags.
    ///
    /// Sample request:
    ///
    ///     DELETE /api/tags/tag/{tagId}
    /// </remarks>
    /// <response code="204">Tag deleted successfully. No content is returned.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to manage tags.</response>
    /// <response code="404">If the current tenant is not found or the tag does not exist.</response>
    [Authorize(Policy = Policies.Tags_Manage)]
    [HttpDelete("tag/{tagId}")]
    public async Task<ActionResult> DeleteTag(string tagId)
    {
        var command = new DeleteTag.Command
        {
            TagId = tagId
        };

        return HandleResult(await Mediator.Send(command));
    }

}
