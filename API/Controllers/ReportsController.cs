using API.Contracts.Reports;
using API.SetupProgram;
using Application.Features.Reports.DTOs;
using Application.Features.Reports.DTOs.Summaries;
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
    [Authorize(Policy = Policies.High_Level_Manage)]
    [HttpGet("tenants/my-tenant/tenant-info-report")]
    public async Task<ActionResult<TenantInfoReport>> GetMyTenantInfoReport()
    {
        var query = new GetTenantInfoReport.Query();

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves a tenant info report as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/tenants/my-tenant/tenant-info-report/pdf
    ///
    /// </remarks>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.High_Level_Manage)]
    [HttpGet("tenants/my-tenant/tenant-info-report/pdf")]
    public async Task<IActionResult> GetMyTenantInfoReportPdf()
    {
        var query = new GetTenantInfoReportPdf.Query();
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
    public async Task<ActionResult<TenantInfoReport>> GetTenantInfoReport(string tenantId)
    {
        var query = new GetTenantInfoReport.Query
        {
            TenantId = tenantId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves a tenant info report as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/tenants/{tenantId}/tenant-info-report/pdf
    ///
    /// </remarks>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.SuperAdmin)]
    [HttpGet("tenants/{tenantId}/tenant-info-report/pdf")]
    public async Task<IActionResult> GetTenantInfoReportPdf(string tenantId)
    {
        var query = new GetTenantInfoReportPdf.Query
        {
            TenantId = tenantId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves a sprints summary report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/projects/{ProjectId}/sprints-summary-report
    ///
    /// </remarks>
    /// <response code="200">Returns the report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("projects/{projectId}/sprints-summary-report")]
    public async Task<ActionResult<SprintsSummary>> GetSprintsSummaryReport(string projectId)
    {
        var query = new GetProjectSprintsSummaryReport.Query()
        {
            ProjectId = projectId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves sprint progress report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/projects/{ProjectId}/sprints/{sprintId}/sprint-progress-report
    ///
    /// </remarks>
    /// <response code="200">Returns the report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("projects/{projectId}/sprints/{sprintId}/sprint-progress-report")]
    public async Task<ActionResult<SprintProgressDto>> GetSprintProgressReport(string projectId, string sprintId)
    {
        var query = new GetSprintsProgressReport.Query()
        {
            ProjectId = projectId,
            SprintId = sprintId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves Sprint Progress report as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/projects/{projectId}/sprints/{sprintId}/sprint-progress-report/pdf
    ///
    /// </remarks>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("projects/{projectId}/sprints/{sprintId}/sprint-progress-report/pdf")]
    public async Task<IActionResult> GetSprintProgressReportPdf(string projectId, string sprintId)
    {
        var query = new GetSprintProgressReportPdf.Query
        {
            ProjectId = projectId,
            SprintId = sprintId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves Project Cycle Time report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/{ProjectId}/cycle-time-report
    ///
    /// </remarks>
    /// <response code="200">Returns the report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("{projectId}/cycle-time-report")]
    public async Task<ActionResult<CycleTimeReport>> GetCycleTimeReport(string projectId, [FromQuery] GetCycleTimeReportRequest request)
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
    ///     GET /api/reports/{ProjectId}/cycle-time-report/pdf
    ///
    /// </remarks>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("{projectId}/cycle-time-report/pdf")]
    public async Task<IActionResult> GetCycleTimeReportPdf(string projectId, [FromQuery] GetCycleTimeReportRequest request)
    {
        var query = Mapper.Map<GetCycleTimeReportPdf.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves Project Velocity report as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/{ProjectId}/velocity-report
    ///
    /// </remarks>
    /// <response code="200">Returns the report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("{projectId}/velocity-report")]
    public async Task<ActionResult<VelocityReportDto>> GetVelocityReport(string projectId)
    {
        var query = new GetProjectVelocityReport.Query
        {
            ProjectId = projectId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves Project Velocity report as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/{projectId}/velocity-report/pdf
    ///
    /// </remarks>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If entities not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("{projectId}/velocity-report/pdf")]
    public async Task<IActionResult> GetVelocityReportPdf(string projectId)
    {
        var query = new GetProjectVelocityReportPdf.Query
        {
            ProjectId = projectId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the bug severity distribution report for a project.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/bug-severity-distribution-report?from=2026-05-01&amp;to=2026-05-31
    ///
    /// This report groups project bugs by severity using the work item priority values:
    /// Low, Medium, High, and Critical.
    /// </remarks>
    /// <param name="request">The report date range query parameters.</param>
    /// <response code="200">Returns the bug severity distribution report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("bug-severity-distribution-report")]
    public async Task<ActionResult<BugSeverityDistributionReportDto>> GetBugSeverityDistributionReport(
        [FromQuery] GetBugSeverityDistributionReportRequest request)
    {
        var query = Mapper.Map<GetBugSeverityDistributionReport.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the bug severity distribution report for a project as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/bug-severity-distribution-report/pdf?from=2026-05-01&amp;to=2026-05-31
    ///
    /// This PDF report visualizes bug counts and percentages by severity.
    /// </remarks>
    /// <param name="request">The report date range query parameters.</param>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("bug-severity-distribution-report/pdf")]
    public async Task<IActionResult> GetBugSeverityDistributionReportPdf(
        [FromQuery] GetBugSeverityDistributionReportRequest request)
    {
        var query = Mapper.Map<GetBugSeverityDistributionReportPdf.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the developer productivity report for a project.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/developer-productivity-report?from=2026-05-01&amp;to=2026-05-31&amp;teamId=team-id
    ///
    /// This report summarizes developer workload, completed work items, completed effort points,
    /// blocked items, overdue items, completion percentage, and average cycle time.
    /// </remarks>
    /// <param name="request">The report date range and optional team filter query parameters.</param>
    /// <response code="200">Returns the developer productivity report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project or team does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("developer-productivity-report")]
    public async Task<ActionResult<DeveloperProductivityReportDto>> GetDeveloperProductivityReport(
        [FromQuery] GetDeveloperProductivityReportRequest request)
    {
        var query = Mapper.Map<GetDeveloperProductivityReport.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the developer productivity report for a project as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/developer-productivity-report/pdf?from=2026-05-01&amp;to=2026-05-31&amp;teamId=team-id
    ///
    /// This PDF report ranks developers by completed effort and highlights delivery risks.
    /// </remarks>
    /// <param name="request">The report date range and optional team filter query parameters.</param>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project or team does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("developer-productivity-report/pdf")]
    public async Task<IActionResult> GetDeveloperProductivityReportPdf(
        [FromQuery] GetDeveloperProductivityReportRequest request)
    {
        var query = Mapper.Map<GetDeveloperProductivityReportPdf.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the teams productivity report for a project.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/teams-productivity-report?from=2026-05-01&amp;to=2026-05-31
    ///
    /// This report summarizes team-level completed work items, completed effort points,
    /// completion percentage, blocked items, overdue items, and average cycle time.
    /// </remarks>
    /// <param name="request">The report date range query parameters.</param>
    /// <response code="200">Returns the teams productivity report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("teams-productivity-report")]
    public async Task<ActionResult<TeamsProductivityReportDto>> GetTeamsProductivityReport(
        [FromQuery] GetTeamsProductivityReportRequest request)
    {
        var query = Mapper.Map<GetTeamsProductivityReport.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the teams productivity report for a project as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/teams-productivity-report/pdf?from=2026-05-01&amp;to=2026-05-31
    ///
    /// This PDF report ranks teams by completed effort and highlights team workload risks.
    /// </remarks>
    /// <param name="request">The report date range query parameters.</param>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("teams-productivity-report/pdf")]
    public async Task<IActionResult> GetTeamsProductivityReportPdf(
        [FromQuery] GetTeamsProductivityReportRequest request)
    {
        var query = Mapper.Map<GetTeamsProductivityReportPdf.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the blocked work items report for a project.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/blocked-work-items-report?from=2026-05-01&amp;to=2026-05-31&amp;teamId=team-id&amp;assignedToUserId=user-id&amp;maxItems=100
    ///
    /// This report lists currently blocked work items and groups them by priority and status.
    /// Optional filters can narrow the result by team or assigned user.
    /// </remarks>
    /// <param name="request">The report date range, optional team/user filters, and max items query parameters.</param>
    /// <response code="200">Returns the blocked work items report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project, team, or user does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("blocked-work-items-report")]
    public async Task<ActionResult<BlockedWorkItemsReportDto>> GetBlockedWorkItemsReport(
        [FromQuery] GetBlockedWorkItemsReportRequest request)
    {
        var query = Mapper.Map<GetBlockedWorkItemsReport.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the blocked work items report for a project as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/blocked-work-items-report/pdf?from=2026-05-01&amp;to=2026-05-31&amp;teamId=team-id&amp;assignedToUserId=user-id&amp;maxItems=100
    ///
    /// This PDF report visualizes blockers by priority, status, and blocked age.
    /// </remarks>
    /// <param name="request">The report date range, optional team/user filters, and max items query parameters.</param>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project, team, or user does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("blocked-work-items-report/pdf")]
    public async Task<IActionResult> GetBlockedWorkItemsReportPdf(
        [FromQuery] GetBlockedWorkItemsReportRequest request)
    {
        var query = Mapper.Map<GetBlockedWorkItemsReportPdf.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the overdue work items report for a project.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/overdue-work-items-report?from=2026-05-01&amp;to=2026-05-31&amp;teamId=team-id&amp;assignedToUserId=user-id&amp;maxItems=100
    ///
    /// This report lists work items whose end date has passed and are not done.
    /// Optional filters can narrow the result by team or assigned user.
    /// </remarks>
    /// <param name="request">The report date range, optional team/user filters, and max items query parameters.</param>
    /// <response code="200">Returns the overdue work items report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project, team, or user does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("overdue-work-items-report")]
    public async Task<ActionResult<OverdueWorkItemsReportDto>> GetOverdueWorkItemsReport(
        [FromQuery] GetOverdueWorkItemsReportRequest request)
    {
        var query = Mapper.Map<GetOverdueWorkItemsReport.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the overdue work items report for a project as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/overdue-work-items-report/pdf?from=2026-05-01&amp;to=2026-05-31&amp;teamId=team-id&amp;assignedToUserId=user-id&amp;maxItems=100
    ///
    /// This PDF report visualizes overdue work items by priority, status, and days overdue.
    /// </remarks>
    /// <param name="request">The report date range, optional team/user filters, and max items query parameters.</param>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project, team, or user does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("overdue-work-items-report/pdf")]
    public async Task<IActionResult> GetOverdueWorkItemsReportPdf(
        [FromQuery] GetOverdueWorkItemsReportRequest request)
    {
        var query = Mapper.Map<GetOverdueWorkItemsReportPdf.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the project health report for a project.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/project-health-report?from=2026-05-01&amp;to=2026-05-31
    ///
    /// This report calculates a health score based on completion percentage, blocked work,
    /// overdue work, critical bugs, and high-priority bugs.
    /// </remarks>
    /// <param name="request">The report date range query parameters.</param>
    /// <response code="200">Returns the project health report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("project-health-report")]
    public async Task<ActionResult<ProjectHealthReportDto>> GetProjectHealthReport(
        [FromQuery] GetProjectHealthReportRequest request)
    {
        var query = Mapper.Map<GetProjectHealthReport.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the project health report for a project as a PDF.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/reports/project-health-report/pdf?from=2026-05-01&amp;to=2026-05-31
    ///
    /// This PDF report visualizes project health score, health status, and risk factors.
    /// </remarks>
    /// <param name="request">The report date range query parameters.</param>
    /// <response code="200">Returns the PDF report.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If the project does not exist.</response>
    [Authorize(Policy = Policies.Tenant_Read)]
    [HttpGet("project-health-report/pdf")]
    public async Task<IActionResult> GetProjectHealthReportPdf(
        [FromQuery] GetProjectHealthReportRequest request)
    {
        var query = Mapper.Map<GetProjectHealthReportPdf.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }
}