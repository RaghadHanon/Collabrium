using API.Contracts.Reports;
using API.SetupProgram;
using Application.Features.Projects.DTOs;
using Application.Features.Reports.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/reports")]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class ReportsController : ApiBaseController
{
    /// <summary>
    /// Retrieves a tenant info summary report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/tenants/my-tenant/tenant-info-report
    ///
    /// </remarks>
    /// <response code="200">Returns the report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.TenantAdmin)]
    [HttpGet("tenants/my-tenant/tenant-info-report")]
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
    ///     GET /api/reports/tenants/{tenantId}/tenant-info-report
    ///
    /// </remarks>
    /// <response code="200">Returns the report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.SuperAdmin)]
    [HttpGet("tenants/{tenantId}/tenant-info-report")]
    public async Task<ActionResult<ProjectDto>> GetTenantInfoReport(string tenantId)
    {
        var query = new GetTenantInfoReport.Query
        {
            TenantId = tenantId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves Project Cycle Time report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/{projectId}/cycle-time-report
    ///
    /// </remarks>
    /// <response code="200">Returns the report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.Projects_Manage)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("{projectId}/cycle-time-report")]
    public async Task<ActionResult<ProjectDto>> GetCycleTimeReport(string projectId, [FromQuery] GetCycleTimeReportRequest request)
    {
        var query = Mapper.Map<GetCycleTimeReport.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves Project Cycle Time report as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/{projectId}/cycle-time-report/pdf
    ///
    /// </remarks>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.Projects_Manage)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("{projectId}/cycle-time-report/pdf")]
    public async Task<IActionResult> GetCycleTimeReportPdf(string projectId, [FromQuery] GetCycleTimeReportRequest request)
    {
        var query = Mapper.Map<GetCycleTimeReportPdf.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }
}