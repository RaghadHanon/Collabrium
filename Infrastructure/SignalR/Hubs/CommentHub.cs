using Application.Common.Exceptions;
using Application.Features.WorkItems.Commands.Comments;
using Application.Features.WorkItems.Queries.Comments;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR.Hubs;

[Authorize]
public sealed class CommentHub(
    IMediator mediator,
    ITenantContext tenantContext
) : Hub
{
    public async Task SendComment(string projectId, string workItemId, string content)
    {
        var (tenantId, commentsGroup) = ValidateAndBuildGroups(projectId, workItemId);

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
        var (_, commentsGroup) = ValidateAndBuildGroups(projectId, workItemId);

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
        var (_, commentsGroup) = ValidateAndBuildGroups(projectId, workItemId);

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
            Context.Abort();
            return;
        }

        var projectId = http.Request.Query["projectId"].ToString();
        var workItemId = http.Request.Query["workItemId"].ToString();

        if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(workItemId))
        {
            await Clients.Caller.SendAsync("Comments:Error", "Missing projectId/workItemId in query string.");
            Context.Abort();
            return;
        }

        var tenantId = tenantContext.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            await Clients.Caller.SendAsync("Comments:Error", "Tenant is not resolved.");
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
            return;
        }

        await Clients.Caller.SendAsync("Comments:Load", result.Value);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    private (string tenantId, string commentsGroup) ValidateAndBuildGroups(string projectId, string workItemId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new HubException("Invalid projectId.");

        if (string.IsNullOrWhiteSpace(workItemId))
            throw new HubException("Invalid workItemId.");

        var tenantId = tenantContext.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new MissingTenantIdException("Tenant Id is missing.");

        return (tenantId, BuildCommentsGroup(tenantId, projectId, workItemId));
    }

    private static string BuildCommentsGroup(string tenantId, string projectId, string workItemId)
        => $"tenants:{tenantId}:projects:{projectId}:workitems:{workItemId}:comments";
}