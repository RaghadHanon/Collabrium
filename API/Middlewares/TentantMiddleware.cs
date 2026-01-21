using API.Common.Attributes;
using Application.Common.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

/// <summary>
/// Middleware that processes HTTP requests to determine the current tenant context based on the request headers or user
/// roles.
/// </summary>
/// <remarks>This middleware checks if the user is authenticated and has the "SuperAdmin" role. If so, it assigns
/// a global tenant context. Otherwise, it attempts to retrieve the tenant information from the "X-Tenant-Id" request
/// header. If the tenant is not found or is inactive, an exception is thrown. The middleware then sets the tenant
/// context and passes control to the next middleware in the pipeline.</remarks>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext,
        ITenantRepository tenantRepository, UserManager<ApplicationUser> userManager)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IgnoreTenantContextAttribute>() != null)
        {
            await _next(context);
            return;
        }

        var user = await userManager.GetUserAsync(context.User);
        if (context.User.Identity?.IsAuthenticated == true)
        {

            if (user != null && await userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                var globalTenant = await tenantRepository
                    .GetByDomainAsync("global.platform");
                tenantContext.TenantId = globalTenant!.Id;
                tenantContext.CurrentTenant = globalTenant;

                await _next(context);
                return;
            }
        }

        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
        {
            var tenantId = tenantIdValues.FirstOrDefault();
            if (tenantId == null)
                throw new UnauthorizedAccessException("X-Tenant-Id header is missing.");

            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                throw new UnauthorizedAccessException("The tenant does not exist.");

            if (user != null && user.TenantId != tenant.Id)
                throw new UnauthorizedAccessException("User does not belong to the specified tenant.");

            if (!tenant.IsActive)
                throw new UnauthorizedAccessException("The tenant is not active.");

            tenantContext.TenantId = tenant.Id;
            tenantContext.CurrentTenant = tenant;
        }
        else
        {
            throw new MissingTenantIdException("Tenant Id is missing in the request header.");
        }

        await _next(context);
    }
}
