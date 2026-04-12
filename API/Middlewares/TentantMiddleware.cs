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
     ITenantRepository tenantRepository)
    {
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IgnoreTenantContextAttribute>() != null)
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("User must be authenticated.");

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
            ?? throw new TenantNotFoundException($"The tenant with id:{tenantId} does not exist.");

        if (!tenant.IsActive)
            throw new TenantNotActiveException("The tenant is not active.");

        tenantContext.TenantId = tenant.Id;
        tenantContext.CurrentTenant = tenant;

        await _next(context);
    }
}
