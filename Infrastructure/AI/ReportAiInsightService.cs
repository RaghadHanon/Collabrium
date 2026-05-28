using System.Text.Json;
using Application.Features.AI.DTOs.Reports;
using Application.Features.AI.Queries;
using Application.Interfaces;
using Application.Interfaces.AI;
using Application.Interfaces.ReadModels;
using Domain.Enums;

namespace Infrastructure.AI;

public sealed class ReportAiInsightService(
    IReportsReadModel reportsReadModel,
    IAiAssistantService aiAssistantService,
    ITenantContext tenantContext)
    : IReportAiInsightService
{
    public async Task<ReportAiInsightDto> GenerateAsync(
        GenerateReportAiInsight.Query query,
        CancellationToken cancellationToken = default)
    {
        var reportData = await GetReportDataAsync(query);

        var prompt = BuildPrompt(
            query.ReportType,
            reportData);

        var insight = await aiAssistantService.AskAsync(
            prompt,
            cancellationToken);

        return new ReportAiInsightDto
        {
            ReportType = query.ReportType,
            Insight = insight,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private async Task<object> GetReportDataAsync(
        GenerateReportAiInsight.Query query)
    {
        var tenantId = tenantContext.TenantId!;

        return query.ReportType switch
        {
            ReportType.TenantInfo =>
                await reportsReadModel.GenerateTenantInfoReportAsync(
                    tenantId),

            ReportType.SprintProgress =>
                await reportsReadModel.GenerateSprintProgressReport(
                    tenantId,
                    query.ProjectId!,
                    query.SprintId!),

            ReportType.Velocity =>
                await reportsReadModel.GenerateVelocityReport(
                    tenantId,
                    query.ProjectId!),

            ReportType.CycleTime =>
                await reportsReadModel.GenerateCycleTimeReportAsync(
                    tenantId,
                    query.ProjectId!,
                    query.From!.Value,
                    query.To!.Value,
                    query.SprintId),

            ReportType.BugSeverityDistribution =>
                await reportsReadModel.GenerateBugSeverityDistributionReportAsync(
                    tenantId,
                    query.From!.Value,
                    query.To!.Value,
                    query.ProjectId),

            ReportType.DeveloperProductivity =>
                await reportsReadModel.GenerateDeveloperProductivityReportAsync(
                    tenantId,
                    query.From!.Value,
                    query.To!.Value,
                    query.ProjectId,
                    query.TeamId,
                    query.UserId),

            ReportType.TeamsProductivity =>
                await reportsReadModel.GenerateTeamsProductivityReportAsync(
                    tenantId,
                    query.From!.Value,
                    query.To!.Value,
                    query.ProjectId),

            ReportType.BlockedWorkItems =>
                await reportsReadModel.GenerateBlockedWorkItemsReportAsync(
                    tenantId,
                    query.From!.Value,
                    query.To!.Value,
                    query.ProjectId,
                    query.TeamId,
                    query.AssignedToUserId,
                    query.MaxItems ?? 100),

            ReportType.OverdueWorkItems =>
                await reportsReadModel.GenerateOverdueWorkItemsReportAsync(
                    tenantId,
                    query.From!.Value,
                    query.To!.Value,
                    query.ProjectId,
                    query.TeamId,
                    query.AssignedToUserId,
                    query.MaxItems ?? 100),

            ReportType.ProjectHealth =>
                await reportsReadModel.GenerateProjectHealthReportAsync(
                    tenantId,
                    query.From!.Value,
                    query.To!.Value,
                    query.ProjectId),

            _ => throw new NotSupportedException(
                $"AI insight is not implemented for report type '{query.ReportType}'.")
        };
    }

    private static string BuildPrompt(
        ReportType reportType,
        object reportData)
    {
        var reportJson = JsonSerializer.Serialize(reportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return $"""
        You are an AI reporting assistant inside Collabrium.

        Analyze the following {reportType} report.

        Rules:
        - Do not invent data.
        - Use only the provided report data.
        - Do not mention internal IDs unless needed.
        - Keep the answer professional, concise, and practical.
        - If there is not enough data, say that clearly.
        - Give recommendations that a project manager or team lead can use.
        - Avoid blaming individuals or teams.
        - Focus on patterns, risks, and actionable improvements.

        Report-specific focus:
        {GetReportSpecificInstructions(reportType)}

        Report data:
        {reportJson}

        Return the answer using this structure:
        1. Summary
        2. Key Findings
        3. Risks
        4. Recommendations
        """;
    }

    private static string GetReportSpecificInstructions(ReportType reportType)
    {
        return reportType switch
        {
            ReportType.TenantInfo =>
                """
                Focus on tenant activity, users, teams, projects, sprints, and work item distribution.
                Explain whether the tenant looks active, organized, and well-structured.
                """,

            ReportType.SprintProgress =>
                """
                Focus on total work items, completed work items, general progress percentage,
                user story progress percentage, and sprint execution risk.
                """,

            ReportType.Velocity =>
                """
                Focus on total story points, completed story points, average velocity,
                and sprint-to-sprint consistency.
                """,

            ReportType.CycleTime =>
                """
                Focus on average cycle time, median cycle time, minimum and maximum cycle time,
                slow delivery patterns, and delivery efficiency.
                """,

            ReportType.BugSeverityDistribution =>
                """
                Focus on total bugs, critical bugs, high bugs, severity percentages,
                and quality or stability risks.
                """,

            ReportType.DeveloperProductivity =>
                """
                Focus on completed work, assigned work, blocked work, overdue work,
                effort points, completion percentage, and average cycle time.
                Do not shame, rank harshly, or negatively label any developer.
                """,

            ReportType.TeamsProductivity =>
                """
                Focus on team-level completion, effort points, blocked work, overdue work,
                capacity, workload distribution, and collaboration risks.
                """,

            ReportType.BlockedWorkItems =>
                """
                Focus on total blocked items, critical and high blocked items,
                blocked age, priority distribution, and what should be unblocked first.
                """,

            ReportType.OverdueWorkItems =>
                """
                Focus on total overdue items, critical and high overdue items,
                days overdue, priority distribution, and schedule risk.
                """,

            ReportType.ProjectHealth =>
                """
                Focus on health score, health status, blocked items, overdue items,
                critical bugs, high bugs, completion percentage, and overall project risk.
                """,

            _ =>
                """
                Focus on the most important metrics, unusual patterns, risks, and practical actions.
                """
        };
    }
}