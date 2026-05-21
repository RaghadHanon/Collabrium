using API.Contracts.Events;
using API.SetupProgram;
using Application.Common;
using Application.Features.Events.Commands;
using Application.Features.Events.DTOs;
using Application.Features.Events.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Route("api/events")]
[ApiController]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class EventsController : ApiBaseController
{
    /// <summary>
    /// Retrieves visible events for users.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/events?search=workshop&amp;isOnline=true&amp;pageSize=10
    ///
    /// This endpoint excludes cancelled events.
    /// </remarks>
    /// <param name="request">Search, filtering, and pagination parameters.</param>
    /// <returns>A paged list of events.</returns>
    /// <response code="200">Events retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read events.</response>
    /// <response code="404">If tenant context is missing.</response>
    [Authorize(Policy = Policies.Events_Read)]
    [HttpGet]
    public async Task<ActionResult<PagedList<EventDto, DateTime?>>> GetEvents(
        [FromQuery] GetEventsRequest request)
    {
        var query = Mapper.Map<GetEvents.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves events for management.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/events/manage?isCancelled=false&amp;pageSize=10
    ///
    /// This endpoint can include cancelled events based on filters.
    /// </remarks>
    /// <param name="request">Search, filtering, and pagination parameters.</param>
    /// <returns>A paged list of events for management.</returns>
    /// <response code="200">Manage events retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to manage events.</response>
    /// <response code="404">If tenant context is missing.</response>
    [Authorize(Policy = Policies.Events_Manage)]
    [HttpGet("manage")]
    public async Task<ActionResult<PagedList<EventDto, DateTime?>>> GetManageEvents(
        [FromQuery] GetManageEventsRequest request)
    {
        var query = Mapper.Map<GetManageEvents.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves a single event by id.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/events/{eventId}
    ///
    /// </remarks>
    /// <param name="eventId">The event id.</param>
    /// <returns>The requested event.</returns>
    /// <response code="200">Event retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read events.</response>
    /// <response code="404">If tenant or event is not found.</response>
    [Authorize(Policy = Policies.Events_Read)]
    [HttpGet("{eventId}")]
    public async Task<ActionResult<EventDto>> GetEventById(string eventId)
    {
        var query = new GetEventById.Query
        {
            EventId = eventId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves event details including attendees.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/events/{eventId}/details
    ///
    /// </remarks>
    /// <param name="eventId">The event id.</param>
    /// <returns>Event details with attendees.</returns>
    /// <response code="200">Event details retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read events.</response>
    /// <response code="404">If tenant or event is not found.</response>
    [Authorize(Policy = Policies.Events_Read)]
    [HttpGet("{eventId}/details")]
    public async Task<ActionResult<EventDetailsDto>> GetEventDetails(string eventId)
    {
        var query = new GetEventDetails.Query
        {
            EventId = eventId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves events for calendar view.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/events/calendar?startsAfter=2026-06-01&amp;startsBefore=2026-06-30
    ///
    /// </remarks>
    /// <param name="startsAfter">Calendar range start date.</param>
    /// <param name="startsBefore">Calendar range end date.</param>
    /// <returns>List of calendar event items.</returns>
    /// <response code="200">Calendar events retrieved successfully.</response>
    /// <response code="400">If startsBefore is not after startsAfter.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read events.</response>
    /// <response code="404">If tenant context is missing.</response>
    [Authorize(Policy = Policies.Events_Read)]
    [HttpGet("calendar")]
    public async Task<ActionResult<List<EventCalendarItemDto>>> GetCalendarEvents(
        [FromQuery] DateTime startsAfter,
        [FromQuery] DateTime startsBefore)
    {
        var query = new GetCalendarEvents.Query
        {
            StartsAfter = startsAfter,
            StartsBefore = startsBefore
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Creates a new event.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/events/event
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "React Workshop",
    ///       "description": "Frontend training session",
    ///       "startAt": "2026-06-01T10:00:00",
    ///       "endAt": "2026-06-01T12:00:00",
    ///       "isOnline": true,
    ///       "meetingUrl": "https://meet.example.com/react-workshop",
    ///       "maxAttendees": 30
    ///     }
    ///
    /// </remarks>
    /// <param name="request">Event creation payload.</param>
    /// <returns>The created event.</returns>
    /// <response code="201">Event created successfully.</response>
    /// <response code="400">If request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to manage events.</response>
    /// <response code="404">If tenant context is missing.</response>
    [Authorize(Policy = Policies.Events_Manage)]
    [HttpPost("event")]
    public async Task<ActionResult<EventDto>> CreateEvent(CreateEventRequest request)
    {
        var command = Mapper.Map<CreateEvent.Command>(request);

        return HandleResult(await Mediator.Send(command), nameof(GetEventById), "Events");

    }

    /// <summary>
    /// Partially updates an existing event.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/events/event/{eventId}
    ///     Content-Type: application/json
    ///
    ///     {
    ///       "title": "Updated React Workshop",
    ///       "startAt": "2026-06-01T11:00:00",
    ///       "endAt": "2026-06-01T13:00:00"
    ///     }
    ///
    /// Only provided fields are updated.
    /// </remarks>
    /// <param name="eventId">The event id.</param>
    /// <param name="request">Partial update payload.</param>
    /// <returns>The updated event.</returns>
    /// <response code="200">Event updated successfully.</response>
    /// <response code="400">If request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to manage events.</response>
    /// <response code="404">If tenant or event is not found.</response>
    [Authorize(Policy = Policies.Events_Manage)]
    [HttpPatch("event/{eventId}")]
    public async Task<ActionResult<EventDto>> UpdateEvent(
        string eventId,
        UpdateEventRequest request)
    {
        var command = Mapper.Map<UpdateEvent.Command>(request);
        command.EventId = eventId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deletes an event.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/events/event/{eventId}
    ///
    /// Deleting an event also deletes its attendance records.
    /// </remarks>
    /// <param name="eventId">The event id.</param>
    /// <response code="200">Event deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to manage events.</response>
    /// <response code="404">If tenant or event is not found.</response>
    [Authorize(Policy = Policies.Events_Manage)]
    [HttpDelete("event/{eventId}")]
    public async Task<ActionResult> DeleteEvent(string eventId)
    {
        var command = new DeleteEvent.Command
        {
            EventId = eventId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Cancels an event without deleting it.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/events/event/{eventId}/cancel
    ///
    /// Cancelled events are hidden from the normal user list but remain visible in management.
    /// </remarks>
    /// <param name="eventId">The event id.</param>
    /// <returns>The cancelled event.</returns>
    /// <response code="200">Event cancelled successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to manage events.</response>
    /// <response code="404">If tenant or event is not found.</response>
    [Authorize(Policy = Policies.Events_Manage)]
    [HttpPatch("event/{eventId}/cancel")]
    public async Task<ActionResult<EventDto>> CancelEvent(string eventId)
    {
        var command = new CancelEvent.Command
        {
            EventId = eventId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Uploads or replaces the event banner image.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/events/event/{eventId}/banner
    ///     Content-Type: multipart/form-data
    ///
    ///     file: banner image
    ///
    /// </remarks>
    /// <param name="eventId">The event id.</param>
    /// <param name="file">The banner image file.</param>
    /// <returns>The event with updated banner image.</returns>
    /// <response code="200">Banner image uploaded successfully.</response>
    /// <response code="400">If no file is uploaded or upload fails.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to manage events.</response>
    /// <response code="404">If tenant or event is not found.</response>
    [Authorize(Policy = Policies.Events_Manage)]
    [HttpPost("event/{eventId}/banner")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<EventDto>> UploadEventBannerImage(
        string eventId,
        IFormFile file)
    {
        var command = new UploadEventBannerImage.Command
        {
            EventId = eventId,
            File = file
        };

        return HandleResult(await Mediator.Send(command));
    }
    /// <summary>
    /// Marks the current user as interested in the event.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/events/{eventId}/attendance
    ///
    /// Interest counts as attendance and enables reminders for the event.
    /// </remarks>
    /// <param name="eventId">The event id.</param>
    /// <returns>The event with updated attendance information.</returns>
    /// <response code="200">Attendance saved successfully.</response>
    /// <response code="400">If the event is cancelled, finished, full, or already attended.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read events.</response>
    /// <response code="404">If tenant or event is not found.</response>
    [Authorize(Policy = Policies.Events_Read)]
    [HttpPost("{eventId}/attendance")]
    public async Task<ActionResult<EventDto>> AttendEvent(string eventId)
    {
        var command = new AttendEvent.Command
        {
            EventId = eventId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Cancels the current user's interest in the event.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/events/{eventId}/attendance
    ///
    /// Removing attendance means the user is no longer interested and will not receive reminders.
    /// </remarks>
    /// <param name="eventId">The event id.</param>
    /// <returns>The event with updated attendance information.</returns>
    /// <response code="200">Attendance cancelled successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read events.</response>
    /// <response code="404">If tenant, event, or attendance is not found.</response>
    [Authorize(Policy = Policies.Events_Read)]
    [HttpDelete("{eventId}/attendance")]
    public async Task<ActionResult<EventDto>> CancelAttendance(string eventId)
    {
        var command = new CancelEventAttendance.Command
        {
            EventId = eventId
        };

        return HandleResult(await Mediator.Send(command));
    }
}