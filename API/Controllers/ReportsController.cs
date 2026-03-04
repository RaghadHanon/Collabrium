using API.Contracts.Projects;
using API.SetupProgram;
using Application.Features.Projects.Commands;
using Application.Features.Projects.DTOs;
using Application.Features.Projects.Queries;
using Application.Features.Reports.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/tenants")]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class ReportsController : ApiBaseController
{
    /// <summary>
    /// Retrieves a tenant info summary report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/tenants/my-tenant/tenant-info-report
    ///
    /// </remarks>
    /// <response code="200">Returns the report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.TenantAdmin)]
    [HttpGet("my-tenant/tenant-info-report")]
    public async Task<ActionResult<ProjectDto>> GetMyTenantInfoReport()
    {
        var query = new GetTenantInfoReport.Query();

        return HandleResult(await Mediator.Send(query));
    }
    
    /// <summary>
    /// Retrieves a tenant info summary report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/tenants/my-tenant/tenant-info-report/{projectId}
    ///
    /// </remarks>
    /// <response code="200">Returns the report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.SuperAdmin)]
    [HttpGet("{tenantId}/tenant-info-report")]
    public async Task<ActionResult<ProjectDto>> GetTenantInfoReport(string tenantId)
    {
        var query = new GetTenantInfoReport.Query
        {
            TenantId = tenantId
        };

        return HandleResult(await Mediator.Send(query));
    }
}