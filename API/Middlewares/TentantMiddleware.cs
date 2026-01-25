using API.Common.Attributes;
using Application.Common.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity;

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
        UserManager<ApplicationUserEntity> userManager)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IgnoreTenantContextAttribute>() != null)
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("User must be authenticated.");

        var user = await userManager.GetUserAsync(context.User)
            ?? throw new UnauthorizedAccessException("Authenticated user not found.");

        if (await userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            var globalTenant = await tenantRepository.GetByDomainAsync("global.platform")
                ?? throw new Exception("Global tenant is not configured.");

            tenantContext.TenantId = globalTenant.Id;
            tenantContext.CurrentTenant = globalTenant;

            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
            throw new MissingTenantIdException("Tenant Id is missing in the request header.");

        var tenantId = tenantIdValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new UnauthorizedAccessException("X-Tenant-Id header is empty.");

        var tenant = await tenantRepository.GetByIdAsync(tenantId)
            ?? throw new UnauthorizedAccessException("The tenant does not exist.");

        if (!tenant.IsActive)
            throw new UnauthorizedAccessException("The tenant is not active.");

        if (user.TenantId != tenant.Id)
            throw new UnauthorizedAccessException("User does not belong to the specified tenant.");

        tenantContext.TenantId = tenant.Id;
        tenantContext.CurrentTenant = tenant;

        await _next(context);
    }
}