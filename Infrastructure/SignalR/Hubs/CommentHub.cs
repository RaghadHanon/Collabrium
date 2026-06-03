using Application.Features.WorkItems.Commands.Comments;
using Application.Features.WorkItems.Queries.Comments;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.SignalR.Hubs;

[Authorize]
public sealed class CommentHub(
    IMediator mediator,
    ITenantContext tenantContext,
    ITenantRepository tenantRepository,
    ILogger<CommentHub> logger
) : Hub
{
    private const string ProjectIdItemKey = "CommentHub.ProjectId";
    private const string WorkItemIdItemKey = "CommentHub.WorkItemId";
    private const string CommentsGroupItemKey = "CommentHub.CommentsGroup";

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

        var tenantId = await TryResolveAndSetTenantContextAsync();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            await Clients.Caller.SendAsync(
                "Comments:Error",
                "Tenant is not resolved from token.",
                Context.ConnectionAborted);

            logger.LogWarning(
                "CommentHub aborted: tenantId missing or invalid. ConnectionId={ConnectionId}",
                Context.ConnectionId);

            Context.Abort();
            return;
        }

        logger.LogInformation(
            "CommentHub connect. ConnectionId={ConnectionId}, TenantId={TenantId}, ProjectId={ProjectId}, WorkItemId={WorkItemId}",
            Context.ConnectionId,
            tenantId,
            projectId,
            workItemId);

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

    public async Task SendComment(string projectId, string workItemId, string content)
    {
        var tenantId = await ResolveAndSetTenantContextOrThrowAsync();

        EnsureSameConnectedWorkItem(projectId, workItemId);

        var commentsGroup = GetConnectedCommentsGroupOrThrow();

        logger.LogInformation(
            "CommentHub SendComment. ConnectionId={ConnectionId}, TenantId={TenantId}, ProjectId={ProjectId}, WorkItemId={WorkItemId}",
            Context.ConnectionId,
            tenantId,
            projectId,
            workItemId);

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
        var tenantId = await ResolveAndSetTenantContextOrThrowAsync();

        EnsureSameConnectedWorkItem(projectId, workItemId);

        var commentsGroup = GetConnectedCommentsGroupOrThrow();

        logger.LogInformation(
            "CommentHub UpdateComment. ConnectionId={ConnectionId}, TenantId={TenantId}, ProjectId={ProjectId}, WorkItemId={WorkItemId}, CommentId={CommentId}",
            Context.ConnectionId,
            tenantId,
            projectId,
            workItemId,
            commentId);

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
        var tenantId = await ResolveAndSetTenantContextOrThrowAsync();

        EnsureSameConnectedWorkItem(projectId, workItemId);

        var commentsGroup = GetConnectedCommentsGroupOrThrow();

        logger.LogInformation(
            "CommentHub DeleteComment. ConnectionId={ConnectionId}, TenantId={TenantId}, ProjectId={ProjectId}, WorkItemId={WorkItemId}, CommentId={CommentId}",
            Context.ConnectionId,
            tenantId,
            projectId,
            workItemId,
            commentId);

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

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = ResolveTenantId();

        logger.LogInformation(
            "CommentHub disconnected. ConnectionId={ConnectionId}, TenantId={TenantId}, Error={Error}",
            Context.ConnectionId,
            tenantId,
            exception?.Message);

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<string?> TryResolveAndSetTenantContextAsync()
    {
        logger.LogInformation(
            "CommentHub claim tenantId = {TenantId}",
            Context.User?.FindFirst("tenantId")?.Value);

        logger.LogInformation(
            "CommentHub scoped tenantContext.TenantId BEFORE set = {TenantId}",
            tenantContext.TenantId);

        if (Context.User?.IsInRole(UserRole.SuperAdmin.ToString()) == true)
        {
            var globalTenant = await tenantContext.GetGlobalTenant(Context.ConnectionAborted);

            if (globalTenant is null)
                return null;

            tenantContext.TenantId = globalTenant.Id;
            tenantContext.CurrentTenant = globalTenant;

            logger.LogInformation(
                "CommentHub SuperAdmin global tenant resolved. TenantId={TenantId}",
                globalTenant.Id);

            return globalTenant.Id;
        }

        var tenantId = ResolveTenantId();

        if (string.IsNullOrWhiteSpace(tenantId))
            return null;

        var tenant = await tenantRepository.GetByIdAsync(tenantId);

        if (tenant is null)
            return null;

        if (!tenant.IsActive)
            return null;

        tenantContext.TenantId = tenant.Id;
        tenantContext.CurrentTenant = tenant;

        logger.LogInformation(
            "CommentHub scoped tenantContext.TenantId AFTER set = {TenantId}",
            tenantContext.TenantId);

        return tenant.Id;
    }

    private async Task<string> ResolveAndSetTenantContextOrThrowAsync()
    {
        var tenantId = await TryResolveAndSetTenantContextAsync();

        return string.IsNullOrWhiteSpace(tenantId) ? throw new HubException("Tenant is not resolved from token.") : tenantId;
    }

    private string? ResolveTenantId()
    {
        return Context.User?.FindFirst("tenantId")?.Value
               ?? tenantContext.TenantId;
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
        return Context.Items.TryGetValue(key, out var value) &&
            value is string text &&
            !string.IsNullOrWhiteSpace(text)
            ? text
            : throw new HubException(errorMessage);
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