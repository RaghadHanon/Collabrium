using API.SetupProgram;
using API.Utilities;
using Application.Features.Reports.DTOs;
using Application.Features.Reports.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/reports")]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class ReportsController : ApiBaseController
{
    /// <summary>
    /// Generates a project analysis report as JSON.
    /// </summary>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<ProjectReportDto>> GetProjectReport(string projectId)
    {
        var query = new GetProjectReport.Query
        {
            ProjectId = projectId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Generates a project analysis report as PDF.
    /// </summary>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("project/{projectId}/pdf")]
    public async Task<ActionResult> GetProjectReportAsPdf(string projectId)
    {
        var query = new GetProjectReport.Query
        {
            ProjectId = projectId
        };

        var result = await Mediator.Send(query);
        if (!result.IsSuccess || result.Value is null)
            return HandleResult(result);

        var report = result.Value;

        var lines = new List<string>
        {
            "Collabrium Project Analysis Report",
            $"Project: {report.ProjectName} ({report.ProjectId})",
            $"Generated At (UTC): {report.GeneratedAtUtc:O}",
            string.Empty,
            "Summary",
            $"- Total Work Items: {report.TotalWorkItems}",
            $"- Blocked Work Items: {report.BlockedWorkItems}",
            $"- Unassigned Work Items: {report.UnassignedWorkItems}",
            $"- Backlog Work Items: {report.BacklogWorkItems}",
            string.Empty,
            "Breakdown by Type"
        };

        lines.AddRange(report.ByType.Select(x => $"- {x.Type}: {x.Count}"));

        lines.Add(string.Empty);
        lines.Add("Breakdown by Status");
        lines.AddRange(report.ByStatus.Select(x => $"- {x.Status}: {x.Count}"));

        lines.Add(string.Empty);
        lines.Add("Breakdown by Priority");
        lines.AddRange(report.ByPriority.Select(x => $"- {x.Priority}: {x.Count}"));

        var pdfBytes = SimplePdfBuilder.BuildReportDocument(lines);
        var fileName = $"project-report-{report.ProjectId}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}
