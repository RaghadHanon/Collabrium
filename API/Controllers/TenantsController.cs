using API.Common.Attributes;
using API.Contracts.Tenants;
using API.SetupProgram;
using Application.Common;
using Application.Features.Tenants.Commands;
using Application.Features.Tenants.DTOs;
using Application.Features.Tenants.Queries;
using Application.Features.Users.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/tenants")]
public class TenantsController : ApiBaseController
{
    /// <summary>
    /// Registers a new tenant and its IT admin (tenant-level global admin).
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/tenant-with-admin
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
    /// <returns>The newly created IT user and tenant details.</returns>
    /// <response code="201">Returns the created tenant and admin.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a SuperAdmin.</response>
    [Authorize(Policy = Policies.SuperAdmin)]
    [HttpPost("tenant-with-admin")]
    public async Task<ActionResult<UserWithTenantDto>> RegisterTenantWithAdmin(RegisterTenantWithAdminRequest request)
    {
        var command = Mapper.Map<RegisterTenantWithAdmin.Command>(request);

        return HandleResult(await Mediator.Send(command), "GetAppUser", "Auth");
    }

    /// <summary>
    /// Updates the details of the currently authenticated tenant, including optional file uploads.
    /// </summary>
    /// <remarks>
    /// This endpoint accepts data via `multipart/form-data`.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/tenants/my-tenant
    ///     Content-RelationType: multipart/form-data
    ///     
    ///     Form Data:
    ///     key: "name", value: "TechFusion Ltd", type: Text
    ///     key: "domain", value: "techfusion.com", type: Text
    ///     key: "description", value: "Software development company", type: Text
    ///     key: "country", value: "Palestine", type: Text
    ///     key: "file", value: [Select image file], type: File
    ///     key: "websiteURL", value: "https://techfusion.com", type: Text
    ///     key: "email", value: "contact@techfusion.com", type: Text
    ///     key: "phoneNumber", value: "0597612345", type: Text
    ///     key: "linkedIn", value: "https://linkedin.com/company/techfusion", type: Text
    ///     key: "facebook", value: "https://facebook.com/techfusion", type: Text
    ///     key: "instagram", value: "https://instagram.com/techfusion", type: Text
    ///     key: "youTube", value: "https://youtube.com/techfusion", type: Text
    /// 
    /// Only the fields provided will be updated; fields left empty will remain unchanged.
    /// </remarks>
    /// <param name="request">The tenant data to update, including optional file uploads.</param>
    /// <returns>The updated tenant details along with associated user information.</returns>
    /// <response code="200">Tenant updated successfully, returning the updated tenant data.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a TenantAdmin.</response>
    /// <response code="404">If the tenant could not be found.</response>
    [Authorize(Policy = Policies.Tenant_Manage)]
    [RejectUnknownFormFields(typeof(UpdateTenantRequest), allowEmptyRequest: false)]
    [HttpPatch("my-tenant")]
    public async Task<ActionResult<UserWithTenantDto>> UpdateTenant([FromForm] UpdateTenantRequest request)
    {
        var command = Mapper.Map<UpdateTenant.Command>(request);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Retrieves the tenant details associated with the currently logged-in user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/tenants/my-tenant
    /// 
    /// </remarks>
    /// <returns>The details of the current user's tenant.</returns>
    /// <response code="200">Returns the tenant details successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    /// <response code="404">If the user is not assigned to a tenant.</response>
    [Authorize(Policy = Policies.PasswordChanged)]
    [HttpGet("my-tenant")]
    public async Task<ActionResult<TenantDto>> GetCurrentTenant()
    {
        var query = new GetTenant.Query();

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the tenant details associated with the currently logged-in user with admin details.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/tenants/my-tenant
    /// 
    /// </remarks>
    /// <returns>The details of the current user's tenant.</returns>
    /// <response code="200">Returns the tenant details successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    /// <response code="404">If the user is not assigned to a tenant.</response>
    [Authorize(Policy = Policies.PasswordChanged)]
    [Authorize(Policy = Policies.Tenant_Manage)]
    [HttpGet("my-tenant-with-admin")]
    public async Task<ActionResult<TenantWithAdminDto>> GetCurrentTenantForAdmin()
    {
        var query = new GetTenantWithAdmin.Query();

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the details of a specific tenant by its ID, if id is not provided then the super admin's tenant will be returned.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/tenants/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the tenant.</param>
    /// <returns>The details of the specified tenant.</returns>
    /// <response code="200">Returns the tenant details successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    /// <response code="404">If the tenant with the specified ID was not found.</response>
    [Authorize(Policy = Policies.SuperAdmin)]
    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDto>> GetTenant(string id)
    {
        var query = new GetTenantWithAdmin.Query();
        query.TenantId = id;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Uploads or updates the profile image (logo) for the current tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint accepts a file upload via `multipart/form-data`.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/tenants/profile-image
    ///     Content-RelationType: multipart/form-data
    ///     
    ///     Form Data:
    ///     key: "file", value: [Select image file], type: File
    /// 
    /// </remarks>
    /// <param name="request">The image file to upload (e.g., PNG, JPG, JPEG).</param>
    /// <returns>A success status if the upload completes.</returns>
    /// <response code="200">If the image was uploaded successfully.</response>
    /// <response code="400">If no file is provided or the file format is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized (must be a Tenant Admin).</response>
    [Authorize(Policy = Policies.Tenant_Manage)]
    [Authorize(Policy = Policies.PasswordChanged)]
    [RejectUnknownFormFields(typeof(UploadImageRequest), allowEmptyRequest: false)]
    [HttpPost("profile-image")]
    public async Task<ActionResult> UploadProfileImage([FromForm] UploadImageRequest request)
    {
        var command = Mapper.Map<UploadTenantProfilePhoto.Command>(request);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Retrieves a paginated list of users belonging to the currently authenticated tenant.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/tenants/my-tenant/users?PageNumber=1&PageSize=10&SearchTerm=john
    /// 
    /// </remarks>
    /// <param name="request">The pagination and filtering parameters (e.g., page size, search term).</param>
    /// <returns>A paged list of users within the current tenant.</returns>
    /// <response code="200">Returns the list of users successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Tenant Admin.</response>
    [Authorize(Policy = Policies.Tenant_Manage)]
    [Authorize(Policy = Policies.PasswordChanged)]
    [HttpGet("my-tenant/users")]
    public async Task<ActionResult<PagedList<UserDto, DateTime?>>> GetMyTenantUsers([FromQuery] GetTenantUsersRequest request)
    {
        var query = Mapper.Map<GetTenantUsers.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves a paginated list of users belonging to a specific tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint is restricted to Super Admins for administrative oversight.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/tenants/3fa85f64-5717-4562-b3fc-2c963f66afa6/users?PageNumber=1&PageSize=10
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the tenant to inspect.</param>
    /// <param name="request">The pagination and filtering parameters.</param>
    /// <returns>A paged list of users within the specified tenant.</returns>
    /// <response code="200">Returns the list of users successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Super Admin.</response>
    /// <response code="404">If the tenant with the specified ID does not exist.</response>
    [Authorize(Policy = Policies.SuperAdmin)]
    [HttpGet("{id}/users")]
    public async Task<ActionResult<PagedList<UserDto, DateTime?>>> GetTenantUsers(string id, [FromQuery] GetTenantUsersRequest request)
    {
        var query = Mapper.Map<GetTenantUsers.Query>(request);
        query.TenantId = id;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves a paginated list of tenants based on the specified filtering criteria.
    /// </summary>
    /// <remarks>
    /// This endpoint is restricted to Super Admins for administrative oversight.  
    /// Tenant context is ignored when processing this request.
    ///
    /// Sample request:
    ///
    ///     GET /api/tenants?NameContains=example&Country=US&PageSize=10
    ///
    /// </remarks>
    /// <param name="request">The pagination and filtering parameters for tenants.</param>
    /// <returns>A paged list of tenants matching the specified filters.</returns>
    /// <response code="200">Returns the list of tenants successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Super Admin.</response>
    [Authorize(Policy = Policies.SuperAdmin)]
    [HttpGet]
    public async Task<ActionResult<PagedList<TenantWithAdminDto, DateTime?>>> GetTenants([FromQuery] GetTenantsRequest request)
    {
        var query = Mapper.Map<GetTenants.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Toggles the active status of a tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint is restricted to Super Admins for administrative control.
    /// It allows enabling or disabling any tenant in the system by its identifier.
    ///
    /// Sample request:
    ///
    ///     PUT /api/tenants/3fa85f64-5717-4562-b3fc-2c963f66afa6/toggle-active
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the tenant whose active status will be toggled.</param>
    /// <returns>No content if the operation succeeds.</returns>
    /// <response code="204">The tenant active status was toggled successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Super Admin.</response>
    /// <response code="404">If the tenant with the specified ID does not exist.</response>
    [Authorize(Policy = Policies.SuperAdmin)]
    [HttpPatch("{id}/toggle-active")]
    public async Task<ActionResult> ToggleTenantActiveStatus(string id)
    {
        var command = new ToggleTenantActiveStatus.Command();
        command.TenantId = id;
        command.IsSuper = true;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deactivate current tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint is restricted to Tenant Admins and affects only the tenant
    /// associated with the current authenticated context.
    ///
    /// Sample request:
    ///
    ///     PUT /api/tenants/my-tenant/deactivate
    ///
    /// </remarks>
    /// <returns>No content if the operation succeeds.</returns>
    /// <response code="204">The tenant active status was deactivated successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Tenant Admin.</response>
    [Authorize(Policy = Policies.TenantAdmin)]
    [Authorize(Policy = Policies.PasswordChanged)]
    [HttpPatch("my-tenant/deactivate")]
    public async Task<ActionResult> DeactivateMyTenant()
    {
        var command = new ToggleTenantActiveStatus.Command();
        command.IsSuper = false;

        return HandleResult(await Mediator.Send(command));
    }
}
