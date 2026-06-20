using API.Contracts.Reports;
using API.SetupProgram;
using Application.Features.AI.DTOs.Reports;
using Application.Features.AI.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/ai/reports")]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class AIReportsController : ApiBaseController
{
    /// <summary>
    /// Generates an AI insight for the current tenant info report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/reports/tenants/my-tenant/tenant-info-report/insight
    ///
    /// </remarks>
    /// <response code="200">Returns the generated AI insight.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If report data was not found.</response>
    [Authorize(Policy = Policies.High_Level_Manage)]
    [HttpGet("tenants/my-tenant/tenant-info-report/insight")]
    public async Task<ActionResult<ReportAiInsightDto>> GetMyTenantInfoReportInsight()
    {
        var query = new GenerateReportAiInsight.Query
        {
            ReportType = ReportType.TenantInfo
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates an AI insight for sprint progress report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/reports/projects/{projectId}/sprints/{sprintId}/sprint-progress-report/insight
    ///
    /// </remarks>
    /// <response code="200">Returns the generated AI insight.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If report data was not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("projects/{projectId}/sprints/{sprintId}/sprint-progress-report/insight")]
    public async Task<ActionResult<ReportAiInsightDto>> GetSprintProgressReportInsight(
        string projectId,
        string sprintId)
    {
        var query = new GenerateReportAiInsight.Query
        {
            ReportType = ReportType.SprintProgress,
            ProjectId = projectId,
            SprintId = sprintId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates an AI insight for project cycle time report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/reports/{projectId}/cycle-time-report/insight?from=2026-05-01&amp;to=2026-05-31
    ///
    /// </remarks>
    /// <param name="projectId">The project id.</param>
    /// <param name="request">The cycle time report request query parameters.</param>
    /// <response code="200">Returns the generated AI insight.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If report data was not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("{projectId}/cycle-time-report/insight")]
    public async Task<ActionResult<ReportAiInsightDto>> GetCycleTimeReportInsight(
        string projectId,
        [FromQuery] GetCycleTimeReportRequest request)
    {
        var query = Mapper.Map<GenerateReportAiInsight.Query>(request);
        query.ProjectId = projectId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates an AI insight for project velocity report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/reports/{projectId}/velocity-report/insight
    ///
    /// </remarks>
    /// <param name="projectId">The project id.</param>
    /// <response code="200">Returns the generated AI insight.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have access to the project.</response>
    /// <response code="404">If report data was not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("{projectId}/velocity-report/insight")]
    public async Task<ActionResult<ReportAiInsightDto>> GetVelocityReportInsight(string projectId)
    {
        var query = new GenerateReportAiInsight.Query
        {
            ReportType = ReportType.Velocity,
            ProjectId = projectId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates an AI insight for bug severity distribution report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/reports/bug-severity-distribution-report/insight?from=2026-05-01&amp;to=2026-05-31&amp;projectId=project-id
    ///
    /// </remarks>
    /// <param name="request">The bug severity report request query parameters.</param>
    /// <response code="200">Returns the generated AI insight.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If report data was not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [HttpGet("bug-severity-distribution-report/insight")]
    public async Task<ActionResult<ReportAiInsightDto>> GetBugSeverityDistributionReportInsight(
        [FromQuery] GetBugSeverityDistributionReportRequest request)
    {
        var query = Mapper.Map<GenerateReportAiInsight.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates an AI insight for developer productivity report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/reports/developer-productivity-report/insight?from=2026-05-01&amp;to=2026-05-31&amp;projectId=project-id&amp;teamId=team-id
    ///
    /// </remarks>
    /// <param name="request">The developer productivity report request query parameters.</param>
    /// <response code="200">Returns the generated AI insight.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If report data was not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [HttpGet("developer-productivity-report/insight")]
    public async Task<ActionResult<ReportAiInsightDto>> GetDeveloperProductivityReportInsight(
        [FromQuery] GetDeveloperProductivityReportRequest request)
    {
        var query = Mapper.Map<GenerateReportAiInsight.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates an AI insight for teams productivity report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/reports/teams-productivity-report/insight?from=2026-05-01&amp;to=2026-05-31&amp;projectId=project-id
    ///
    /// </remarks>
    /// <param name="request">The teams productivity report request query parameters.</param>
    /// <response code="200">Returns the generated AI insight.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If report data was not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [HttpGet("teams-productivity-report/insight")]
    public async Task<ActionResult<ReportAiInsightDto>> GetTeamsProductivityReportInsight(
        [FromQuery] GetTeamsProductivityReportRequest request)
    {
        var query = Mapper.Map<GenerateReportAiInsight.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates an AI insight for blocked work items report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/reports/blocked-work-items-report/insight?from=2026-05-01&amp;to=2026-05-31&amp;projectId=project-id&amp;teamId=team-id&amp;assignedToUserId=user-id&amp;maxItems=100
    ///
    /// </remarks>
    /// <param name="request">The blocked work items report request query parameters.</param>
    /// <response code="200">Returns the generated AI insight.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If report data was not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [HttpGet("blocked-work-items-report/insight")]
    public async Task<ActionResult<ReportAiInsightDto>> GetBlockedWorkItemsReportInsight(
        [FromQuery] GetBlockedWorkItemsReportRequest request)
    {
        var query = Mapper.Map<GenerateReportAiInsight.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates an AI insight for overdue work items report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/reports/overdue-work-items-report/insight?from=2026-05-01&amp;to=2026-05-31&amp;projectId=project-id&amp;teamId=team-id&amp;assignedToUserId=user-id&amp;maxItems=100
    ///
    /// </remarks>
    /// <param name="request">The overdue work items report request query parameters.</param>
    /// <response code="200">Returns the generated AI insight.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If report data was not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [HttpGet("overdue-work-items-report/insight")]
    public async Task<ActionResult<ReportAiInsightDto>> GetOverdueWorkItemsReportInsight(
        [FromQuery] GetOverdueWorkItemsReportRequest request)
    {
        var query = Mapper.Map<GenerateReportAiInsight.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates an AI insight for project health report.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/ai/reports/project-health-report/insight?from=2026-05-01&amp;to=2026-05-31&amp;projectId=project-id
    ///
    /// </remarks>
    /// <param name="request">The project health report request query parameters.</param>
    /// <response code="200">Returns the generated AI insight.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If report data was not found.</response>
    [Authorize(Policy = Policies.Projects_Read)]
    [HttpGet("project-health-report/insight")]
    public async Task<ActionResult<ReportAiInsightDto>> GetProjectHealthReportInsight(
        [FromQuery] GetProjectHealthReportRequest request)
    {
        var query = Mapper.Map<GenerateReportAiInsight.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }
}