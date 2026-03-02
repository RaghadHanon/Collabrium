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
    /// Jira-like project analytics report in JSON.
    /// </summary>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("project/{projectId}/analysis")]
    public async Task<ActionResult<ProjectReportDto>> GetProjectReport(
        string projectId,
        [FromQuery] DateTime? startDateUtc,
        [FromQuery] DateTime? endDateUtc)
    {
        var query = new GetProjectReport.Query
        {
            ProjectId = projectId,
            StartDateUtc = startDateUtc,
            EndDateUtc = endDateUtc
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Jira-like project analytics report exported as PDF.
    /// </summary>
    [Authorize(Policy = Policies.Projects_Read)]
    [Authorize(Policy = Policies.Project_Access)]
    [HttpGet("project/{projectId}/analysis/pdf")]
    public async Task<ActionResult> GetProjectReportAsPdf(
        string projectId,
        [FromQuery] DateTime? startDateUtc,
        [FromQuery] DateTime? endDateUtc)
    {
        var query = new GetProjectReport.Query
        {
            ProjectId = projectId,
            StartDateUtc = startDateUtc,
            EndDateUtc = endDateUtc
        };

        var result = await Mediator.Send(query);
        if (!result.IsSuccess || result.Value is null)
            return HandleResult(result);

        var report = result.Value;

        var lines = new List<string>
        {
            "Collabrium Jira-Like Analytics Report",
            $"Project: {report.ProjectName} ({report.ProjectKey})",
            $"Generated At (UTC): {report.GeneratedAtUtc:O}",
            $"Window: {report.Window.StartDateUtc:O} -> {report.Window.EndDateUtc:O}",
            string.Empty,
            "Overview",
            $"- Total: {report.Overview.TotalWorkItems}",
            $"- Done: {report.Overview.DoneWorkItems}",
            $"- In Progress: {report.Overview.InProgressWorkItems}",
            $"- Blocked: {report.Overview.BlockedWorkItems}",
            $"- Backlog: {report.Overview.BacklogWorkItems}",
            $"- Unassigned: {report.Overview.UnassignedWorkItems}",
            $"- Completion Rate: {report.Overview.CompletionRatePercent}%",
            $"- Blocked Rate: {report.Overview.BlockedRatePercent}%",
            string.Empty,
            "Throughput",
            $"- Created In Window: {report.Throughput.CreatedInWindow}",
            $"- Completed In Window: {report.Throughput.CompletedInWindow}",
            $"- Net Flow: {report.Throughput.NetFlow}",
            string.Empty,
            "Sprint Health"
        };

        lines.AddRange(report.SprintHealth.Select(x =>
            $"- {x.SprintName}: scope={x.ScopeCount}, done={x.DoneCount}, completion={x.CompletionPercent}%"));

        lines.Add(string.Empty);
        lines.Add("Top Assignee Workload");
        lines.AddRange(report.AssigneeWorkload.Take(8).Select(x =>
            $"- {x.DisplayName}: total={x.TotalAssigned}, in-progress={x.InProgressCount}, blocked={x.BlockedCount}, done={x.DoneCount}"));

        lines.Add(string.Empty);
        lines.Add("Aging (Open Items)");
        lines.Add($"- 0-7 days: {report.Aging.Fresh0To7Days}");
        lines.Add($"- 8-30 days: {report.Aging.Warm8To30Days}");
        lines.Add($"- 31-60 days: {report.Aging.Old31To60Days}");
        lines.Add($"- 60+ days: {report.Aging.Stale60PlusDays}");

        lines.Add(string.Empty);
        lines.Add("Risk Flags");
        lines.AddRange(report.RiskFlags.DefaultIfEmpty("- None").Select(x => $"- {x}"));

        lines.Add(string.Empty);
        lines.Add("Recommendations");
        lines.AddRange(report.Recommendations.Select(x => $"- {x}"));

        var pdfBytes = SimplePdfBuilder.BuildReportDocument(lines);
        var fileName = $"project-analysis-{report.ProjectKey}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}
