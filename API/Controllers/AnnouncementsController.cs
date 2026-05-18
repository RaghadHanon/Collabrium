using API.Contracts.Announcements;
using API.SetupProgram;
using Application.Common;
using Application.Features.Announcements.Commands;
using Application.Features.Announcements.DTOs;
using Application.Features.Announcements.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Route("api/announcements")]
[ApiController]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class AnnouncementsController : ApiBaseController
{
    /// <summary>
    /// Retrieves announcements visible to the current user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/announcements?scope=All&amp;pageSize=10
    ///
    /// Supported scope values:
    /// - All
    /// - General
    /// - MyTeams
    /// - MyRole
    ///
    /// </remarks>
    /// <response code="200">Announcements retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant not found.</response>
    [Authorize(Policy = Policies.Announcements_Read)]
    [HttpGet]
    public async Task<ActionResult<PagedList<AnnouncementDto, DateTime?>>> GetAnnouncements(
        [FromQuery] GetAnnouncementsRequest request)
    {
        var query = Mapper.Map<GetAnnouncements.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves all announcements for management, including scheduled and expired announcements.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/announcements/manage?pageSize=10
    ///
    /// </remarks>
    /// <response code="200">Announcements retrieved successfully for management.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to manage announcements.</response>
    /// <response code="404">If tenant not found.</response>
    [Authorize(Policy = Policies.Announcements_Manage)]
    [HttpGet("manage")]
    public async Task<ActionResult<PagedList<AnnouncementDto, DateTime?>>> GetManageAnnouncements(
        [FromQuery] GetManageAnnouncementsRequest request)
    {
        var query = Mapper.Map<GetManageAnnouncements.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves an announcement by id.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/announcements/{announcementId}
    ///
    /// </remarks>
    /// <param name="announcementId">The announcement id.</param>
    /// <response code="200">Announcement retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant or announcement not found.</response>
    [Authorize(Policy = Policies.Announcements_Read)]
    [HttpGet("{announcementId}")]
    public async Task<ActionResult<AnnouncementDto>> GetAnnouncementById(string announcementId)
    {
        var query = new GetAnnouncementById.Query
        {
            AnnouncementId = announcementId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves announcement details, including audience targeting information.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/announcements/{announcementId}/details
    ///
    /// </remarks>
    /// <param name="announcementId">The announcement id.</param>
    /// <response code="200">Announcement details retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant or announcement not found.</response>
    [Authorize(Policy = Policies.Announcements_Read)]
    [HttpGet("{announcementId}/details")]
    public async Task<ActionResult<AnnouncementDetailsDto>> GetAnnouncementDetails(string announcementId)
    {
        var query = new GetAnnouncementDetails.Query
        {
            AnnouncementId = announcementId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Creates a new announcement.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/announcements/announcement
    ///     {
    ///       "title": "System Maintenance",
    ///       "content": "The system will be unavailable tonight.",
    ///       "priority": "Important",
    ///       "publishAt": "2026-05-20T09:00:00Z",
    ///       "expiresAt": "2026-05-21T09:00:00Z",
    ///       "audiences": [
    ///         {
    ///           "type": "Everyone"
    ///         }
    ///       ]
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Announcement created successfully.</response>
    /// <response code="400">If request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to create announcements.</response>
    /// <response code="404">If tenant or current user not found.</response>
    [Authorize(Policy = Policies.Announcements_Manage)]
    [HttpPost("announcement")]
    public async Task<ActionResult<AnnouncementDetailsDto>> CreateAnnouncement(
        CreateAnnouncementRequest request)
    {
        var command = Mapper.Map<CreateAnnouncement.Command>(request);

        return HandleResult(
            await Mediator.Send(command),
            nameof(GetAnnouncementDetails),
            "Announcements");
    }

    /// <summary>
    /// Updates an existing announcement using patch semantics.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/announcements/announcement/{announcementId}
    ///     {
    ///       "title": "Updated announcement title",
    ///       "content": "Updated content",
    ///       "priority": "Normal",
    ///       "clearExpiresAt": false
    ///     }
    ///
    /// Null fields are ignored. To remove ExpiresAt, send clearExpiresAt = true.
    /// If audiences is provided, it replaces the announcement audiences.
    ///
    /// </remarks>
    /// <param name="announcementId">The announcement id.</param>
    /// <response code="200">Announcement updated successfully.</response>
    /// <response code="400">If request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to update announcements.</response>
    /// <response code="404">If tenant or announcement not found.</response>
    [Authorize(Policy = Policies.Announcements_Manage)]
    [HttpPatch("announcement/{announcementId}")]
    public async Task<ActionResult<AnnouncementDetailsDto>> UpdateAnnouncement(
        string announcementId,
        UpdateAnnouncementRequest request)
    {
        var command = Mapper.Map<UpdateAnnouncement.Command>(request);
        command.AnnouncementId = announcementId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deletes an announcement.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/announcements/announcement/{announcementId}
    ///
    /// </remarks>
    /// <param name="announcementId">The announcement id.</param>
    /// <response code="200">Announcement deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to delete announcements.</response>
    /// <response code="404">If tenant or announcement not found.</response>
    [Authorize(Policy = Policies.Announcements_Manage)]
    [HttpDelete("announcement/{announcementId}")]
    public async Task<ActionResult<string>> DeleteAnnouncement(string announcementId)
    {
        var command = new DeleteAnnouncement.Command
        {
            AnnouncementId = announcementId
        };

        return HandleResult(await Mediator.Send(command));
    }
}