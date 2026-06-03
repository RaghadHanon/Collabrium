using Application.Features.WorkItems.Commands.Comments;
using Application.Features.WorkItems.Queries.Comments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.SignalR.Hubs;

[Authorize]
public sealed class CommentHub(
    IMediator mediator,
    ILogger<CommentHub> logger
) : Hub
{
    private const string ProjectIdItemKey = "CommentHub.ProjectId";
    private const string WorkItemIdItemKey = "CommentHub.WorkItemId";
    private const string CommentsGroupItemKey = "CommentHub.CommentsGroup";

    public async Task SendComment(string projectId, string workItemId, string content)
    {
        EnsureSameConnectedWorkItem(projectId, workItemId);

        var commentsGroup = GetConnectedCommentsGroupOrThrow();

        var result = await mediator.Send(new AddWorkItemComment.Command
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
            Content = content
        }, Context.ConnectionAborted);

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync(
                "Comments:Error",
                result.Error ?? "Failed to add comment.",
                Context.ConnectionAborted);

            return;
        }

        await Clients.Group(commentsGroup).SendAsync(
            "Comments:Created",
            result.Value,
            Context.ConnectionAborted);
    }

    public async Task UpdateComment(string projectId, string workItemId, string commentId, string content)
    {
        EnsureSameConnectedWorkItem(projectId, workItemId);

        var commentsGroup = GetConnectedCommentsGroupOrThrow();

        var result = await mediator.Send(new UpdateWorkItemComment.Command
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
            CommentId = commentId,
            Content = content
        }, Context.ConnectionAborted);

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync(
                "Comments:Error",
                result.Error ?? "Failed to update comment.",
                Context.ConnectionAborted);

            return;
        }

        await Clients.Group(commentsGroup).SendAsync(
            "Comments:Updated",
            result.Value,
            Context.ConnectionAborted);
    }

    public async Task DeleteComment(string projectId, string workItemId, string commentId)
    {
        EnsureSameConnectedWorkItem(projectId, workItemId);

        var commentsGroup = GetConnectedCommentsGroupOrThrow();

        var result = await mediator.Send(new DeleteWorkItemComment.Command
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
            CommentId = commentId
        }, Context.ConnectionAborted);

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync(
                "Comments:Error",
                result.Error ?? "Failed to delete comment.",
                Context.ConnectionAborted);

            return;
        }

        await Clients.Group(commentsGroup).SendAsync(
            "Comments:Deleted",
            new { CommentId = commentId },
            Context.ConnectionAborted);
    }

    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();

        if (http is null)
        {
            logger.LogWarning(
                "CommentHub aborted: HttpContext is null. ConnectionId={ConnectionId}",
                Context.ConnectionId);

            Context.Abort();
            return;
        }

        var projectId = GetQueryValue(http, "projectId", "ProjectId");
        var workItemId = GetQueryValue(http, "workItemId", "WorkItemId");

        if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(workItemId))
        {
            await Clients.Caller.SendAsync(
                "Comments:Error",
                "Missing projectId/workItemId in query string.",
                Context.ConnectionAborted);

            logger.LogWarning(
                "CommentHub aborted: missing projectId/workItemId. ConnectionId={ConnectionId}, ProjectId={ProjectId}, WorkItemId={WorkItemId}",
                Context.ConnectionId,
                projectId,
                workItemId);

            Context.Abort();
            return;
        }

        var tenantId = GetTenantId();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            await Clients.Caller.SendAsync(
                "Comments:Error",
                "Tenant is not resolved from token.",
                Context.ConnectionAborted);

            logger.LogWarning(
                "CommentHub aborted: tenantId missing. ConnectionId={ConnectionId}",
                Context.ConnectionId);

            Context.Abort();
            return;
        }

        /*
         * Important:
         * Load comments first.
         * If this query fails because the work item does not exist,
         * belongs to another tenant/project, or the user is unauthorized,
         * the connection must NOT be added to the SignalR group.
         */
        var result = await mediator.Send(new GetWorkItemComments.Query
        {
            ProjectId = projectId,
            WorkItemId = workItemId
        }, Context.ConnectionAborted);

        if (!result.IsSuccess || result.Value is null)
        {
            await Clients.Caller.SendAsync(
                "Comments:Error",
                result.Error ?? "Failed to load comments.",
                Context.ConnectionAborted);

            logger.LogWarning(
                "CommentHub aborted: failed loading comments. ConnectionId={ConnectionId}, TenantId={TenantId}, ProjectId={ProjectId}, WorkItemId={WorkItemId}, Error={Error}",
                Context.ConnectionId,
                tenantId,
                projectId,
                workItemId,
                result.Error);

            Context.Abort();
            return;
        }

        var commentsGroup = BuildCommentsGroup(tenantId, projectId, workItemId);

        Context.Items[ProjectIdItemKey] = projectId;
        Context.Items[WorkItemIdItemKey] = workItemId;
        Context.Items[CommentsGroupItemKey] = commentsGroup;

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            commentsGroup,
            Context.ConnectionAborted);

        await Clients.Caller.SendAsync(
            "Comments:Load",
            result.Value,
            Context.ConnectionAborted);

        logger.LogInformation(
            "CommentHub connected. ConnectionId={ConnectionId}, TenantId={TenantId}, ProjectId={ProjectId}, WorkItemId={WorkItemId}, Group={Group}",
            Context.ConnectionId,
            tenantId,
            projectId,
            workItemId,
            commentsGroup);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation(
            "CommentHub disconnected. ConnectionId={ConnectionId}, Error={Error}",
            Context.ConnectionId,
            exception?.Message);

        await base.OnDisconnectedAsync(exception);
    }

    private void EnsureSameConnectedWorkItem(string projectId, string workItemId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new HubException("Invalid projectId.");

        if (string.IsNullOrWhiteSpace(workItemId))
            throw new HubException("Invalid workItemId.");

        var connectedProjectId = GetConnectionItemOrThrow(
            ProjectIdItemKey,
            "Connection projectId is missing.");

        var connectedWorkItemId = GetConnectionItemOrThrow(
            WorkItemIdItemKey,
            "Connection workItemId is missing.");

        var isSameProject = string.Equals(
            connectedProjectId,
            projectId,
            StringComparison.OrdinalIgnoreCase);

        var isSameWorkItem = string.Equals(
            connectedWorkItemId,
            workItemId,
            StringComparison.OrdinalIgnoreCase);

        if (!isSameProject || !isSameWorkItem)
            throw new HubException("This connection is not joined to the requested work item comments group.");
    }

    private string GetConnectedCommentsGroupOrThrow()
    {
        return GetConnectionItemOrThrow(
            CommentsGroupItemKey,
            "Connection comments group is missing.");
    }

    private string GetConnectionItemOrThrow(string key, string errorMessage)
    {
        if (Context.Items.TryGetValue(key, out var value) &&
            value is string text &&
            !string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        throw new HubException(errorMessage);
    }

    private string? GetTenantId()
    {
        return Context.User?.FindFirst("tenantId")?.Value;
    }

    private static string? GetQueryValue(HttpContext http, params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = http.Request.Query[key].ToString();

            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static string BuildCommentsGroup(string tenantId, string projectId, string workItemId)
    {
        return $"tenants:{tenantId}:projects:{projectId}:workitems:{workItemId}:comments";
    }
}