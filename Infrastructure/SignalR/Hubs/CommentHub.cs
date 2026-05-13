using Application.Features.WorkItems.Commands.Comments;
using Application.Features.WorkItems.Queries.Comments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.SignalR.Hubs;

[Authorize]
public sealed class CommentHub(
    IMediator mediator,
    ILogger<CommentHub> logger
) : Hub
{
    public async Task SendComment(string projectId, string workItemId, string content)
    {
        var (tenantId, commentsGroup) = GetTenantAndGroupOrThrow(projectId, workItemId);

        var result = await mediator.Send(new AddWorkItemComment.Command
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
            Content = content
        });

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("Comments:Error", result.Error ?? "Failed to add comment.");
            return;
        }

        await Clients.Group(commentsGroup)
            .SendAsync("Comments:Created", result.Value);
    }

    public async Task UpdateComment(string projectId, string workItemId, string commentId, string content)
    {
        var (_, commentsGroup) = GetTenantAndGroupOrThrow(projectId, workItemId);

        var result = await mediator.Send(new UpdateWorkItemComment.Command
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
            CommentId = commentId,
            Content = content
        });

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("Comments:Error", result.Error ?? "Failed to update comment.");
            return;
        }

        await Clients.Group(commentsGroup)
            .SendAsync("Comments:Updated", result.Value);
    }

    public async Task DeleteComment(string projectId, string workItemId, string commentId)
    {
        var (_, commentsGroup) = GetTenantAndGroupOrThrow(projectId, workItemId);

        var result = await mediator.Send(new DeleteWorkItemComment.Command
        {
            ProjectId = projectId,
            WorkItemId = workItemId,
            CommentId = commentId
        });

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("Comments:Error", result.Error ?? "Failed to delete comment.");
            return;
        }

        await Clients.Group(commentsGroup)
            .SendAsync("Comments:Deleted", new { CommentId = commentId });
    }

    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        if (http is null)
        {
            logger.LogWarning("CommentHub aborted: HttpContext is null. ConnectionId={ConnectionId}", Context.ConnectionId);
            Context.Abort();
            return;
        }

        // اسمين محتملين لتجنب مشاكل اختلاف حالة الأحرف أو التسمية من الفرونت
        var projectId = http.Request.Query["projectId"].ToString();
        if (string.IsNullOrWhiteSpace(projectId))
            projectId = http.Request.Query["ProjectId"].ToString();

        var workItemId = http.Request.Query["workItemId"].ToString();
        if (string.IsNullOrWhiteSpace(workItemId))
            workItemId = http.Request.Query["WorkItemId"].ToString();

        if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(workItemId))
        {
            await Clients.Caller.SendAsync("Comments:Error", "Missing projectId/workItemId in query string.");
            logger.LogWarning(
                "CommentHub aborted: missing projectId/workItemId. ConnectionId={ConnectionId}, projectId={ProjectId}, workItemId={WorkItemId}",
                Context.ConnectionId, projectId, workItemId);
            Context.Abort();
            return;
        }

        var tenantId = GetTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            await Clients.Caller.SendAsync("Comments:Error", "Tenant is not resolved from token.");
            logger.LogWarning("CommentHub aborted: tenantId missing. ConnectionId={ConnectionId}", Context.ConnectionId);
            Context.Abort();
            return;
        }

        var commentsGroup = BuildCommentsGroup(tenantId, projectId, workItemId);

        await Groups.AddToGroupAsync(Context.ConnectionId, commentsGroup);

        var result = await mediator.Send(new GetWorkItemComments.Query
        {
            ProjectId = projectId,
            WorkItemId = workItemId
        });

        if (!result.IsSuccess || result.Value is null)
        {
            await Clients.Caller.SendAsync("Comments:Error", result.Error ?? "Failed to load comments.");
            logger.LogWarning(
                "CommentHub failed loading comments. ConnectionId={ConnectionId}, projectId={ProjectId}, workItemId={WorkItemId}",
                Context.ConnectionId, projectId, workItemId);
            return;
        }

        await Clients.Caller.SendAsync("Comments:Load", result.Value);

        logger.LogInformation(
            "CommentHub connected. ConnectionId={ConnectionId}, TenantId={TenantId}, ProjectId={ProjectId}, WorkItemId={WorkItemId}",
            Context.ConnectionId, tenantId, projectId, workItemId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation(
            "CommentHub disconnected. ConnectionId={ConnectionId}, Error={Error}",
            Context.ConnectionId, exception?.Message);

        await base.OnDisconnectedAsync(exception);
    }

    private (string tenantId, string commentsGroup) GetTenantAndGroupOrThrow(string projectId, string workItemId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new HubException("Invalid projectId.");

        if (string.IsNullOrWhiteSpace(workItemId))
            throw new HubException("Invalid workItemId.");

        var tenantId = GetTenantId();
        return string.IsNullOrWhiteSpace(tenantId)
            ? throw new HubException("Tenant Id is missing from token.")
            : ((string tenantId, string commentsGroup)) (tenantId, BuildCommentsGroup(tenantId, projectId, workItemId));
    }

    private string? GetTenantId()
    {
        return Context.User?.FindFirst("tenantId")?.Value;
    }

    private static string BuildCommentsGroup(string tenantId, string projectId, string workItemId)
    {
        return $"tenants:{tenantId}:projects:{projectId}:workitems:{workItemId}:comments";
    }
}