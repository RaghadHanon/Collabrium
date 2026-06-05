using API.Contracts.Notifications;
using API.SetupProgram;
using Application.Features.Notifications.Commands;
using Application.Features.Notifications.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/notifications")]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class NotificationsController : ApiBaseController
{
    /// <summary>
    /// Retrieves the notifications of the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/notifications?pageSize=10
    ///     GET /api/notifications?pageSize=10&isRead=false
    ///     GET /api/notifications?pageSize=10&cursor=2026-06-05T10:30:00Z
    ///
    /// </remarks>
    /// <param name="request">Notification filtering and pagination parameters.</param>
    /// <returns>A paged list of the current user's notifications.</returns>
    /// <response code="200">Returns notifications successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] GetMyNotificationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = Mapper.Map<GetMyNotifications.Query>(request);

        return HandleResult(await Mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Retrieves the unread notifications count for the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/notifications/unread-count
    ///
    /// </remarks>
    /// <returns>The number of unread notifications.</returns>
    /// <response code="200">Returns unread count successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(
        CancellationToken cancellationToken)
    {
        var query = new GetUnreadNotificationsCount.Query();

        return HandleResult(await Mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Marks a specific notification as read for the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/notifications/3fa85f64-5717-4562-b3fc-2c963f66afa6/read
    ///
    /// </remarks>
    /// <param name="notificationId">The notification ID to mark as read.</param>
    /// <returns>A success message if the notification was marked as read.</returns>
    /// <response code="200">Notification marked as read successfully.</response>
    /// <response code="400">If the notification ID is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    /// <response code="404">If the tenant or current user was not found.</response>
    [HttpPatch("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(
        string notificationId,
        CancellationToken cancellationToken)
    {
        var command = new MarkNotificationAsRead.Command
        {
            NotificationId = notificationId
        };

        return HandleResult(await Mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Marks all notifications as read for the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/notifications/read-all
    ///
    /// </remarks>
    /// <returns>A success message if all notifications were marked as read.</returns>
    /// <response code="200">All notifications marked as read successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    /// <response code="404">If the tenant or current user was not found.</response>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        var command = new MarkAllNotificationsAsRead.Command();

        return HandleResult(await Mediator.Send(command, cancellationToken));
    }
}