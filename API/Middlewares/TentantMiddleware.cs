using API.Common.Attributes;
using Application.Common.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Enums;
using System.Security.Claims;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        ITenantRepository tenantRepository,
        IUsersRepository usersRepository)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User identifier claim is missing.");
        }

        var user = await usersRepository.GetByIdAsync(userId, context.RequestAborted);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Authenticated user does not exist.");
        }

        if (!user.IsActive)
        {
            throw new UserNotActiveException("Your account has been deactivated. Please contact your administrator.");
        }

        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata
                .GetMetadata<IgnoreTenantContextAttribute>() is not null)
        {
            await _next(context);
            return;
        }

        if (context.User.IsInRole(UserRole.SuperAdmin.ToString()))
        {
            var globalTenant = await tenantContext.GetGlobalTenant() 
                ?? throw new Exception("Global tenant is not configured.");

            tenantContext.TenantId = globalTenant.Id;
            tenantContext.CurrentTenant = globalTenant;

            await _next(context);
            return;
        }

        var tenantId = context.User.FindFirstValue("tenantId")
            ?? throw new UnauthorizedAccessException("Tenant claim (tenantId) is missing.");

        var tenant = await tenantRepository.GetByIdAsync(tenantId)
            ?? throw new TenantNotFoundException($"The tenant with id: {tenantId} does not exist.");

        if (!tenant.IsActive)
        {
            throw new TenantNotActiveException("The tenant is not active.");
        }

        if (!string.Equals(user.TenantId, tenant.Id, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("The authenticated user does not belong to this tenant.");
        }

        tenantContext.TenantId = tenant.Id;
        tenantContext.CurrentTenant = tenant;

        await _next(context);
    }
}