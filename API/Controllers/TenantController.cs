using API.Contracts.Tenants;
using API.SetupProgram;
using Application.Features.Identity.DTOs;
using Application.Features.Tenants.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class TenantController : ApiBaseController
{
    /// <summary>
    /// Registers a new tenant and its OrganizationIT admin (tenant-level global admin).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/register-tenant
    ///     {
    ///         "tenantName": "TechFusion Ltd",
    ///         "domain": "techfusion.com",
    ///         "description": "Software development company",
    ///         "fullName": "Omar Khalil",
    ///         "email": "omar@techfusion.com",
    ///         "password": "Pa$$w0rd",
    ///         "confirmPassword": "Pa$$w0rd",
    ///         "dateOfBirth": "1995-06-15",
    ///         "phoneNumber": "0597612345"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The registration data for the new tenant and its admin.</param>
    /// <returns>The newly created OrganizationIT user and tenant details.</returns>
    /// <response code="201">Returns the created tenant and admin.</response>
    /// <response code="400">If the request data is invalid.</response>
    [Authorize(Policy = Policies.SuperAdmin)]
    [HttpPost("register-tenant")]
    public async Task<ActionResult<AppUserDto>> RegisterTenantWithAdmin(RegisterTenantWithAdminRequest request)
    {
        var command = Mapper.Map<RegisterTenantWithAdmin.Command>(request);

        return HandleResult(await Mediator.Send(command), "GetAppUser", "Auth");
    }
}
