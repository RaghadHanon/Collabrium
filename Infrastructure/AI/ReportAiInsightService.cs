using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public async Task<ReportAiInsightDto> GenerateAsync(
        GenerateReportAiInsight.Query query,
        CancellationToken cancellationToken = default)
    {
        var reportData = await GetReportDataAsync(query);

        var prompt = BuildPrompt(
            query.ReportType,
            reportData);

        var aiResponse = await aiAssistantService.AskAsync(
            prompt,
            cancellationToken);

        var insight = ParseInsight(
            query.ReportType,
            aiResponse);

        insight.ReportType = query.ReportType;
        insight.GeneratedAt = DateTime.UtcNow;

        return insight;
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

        return $$"""
        You are an AI reporting assistant inside Collabrium.

        Analyze the following {{reportType}} report.

        Rules:
        - Return valid JSON only.
        - Do not wrap the JSON in markdown.
        - Do not add explanations before or after the JSON.
        - Do not use markdown syntax such as **, *, #, or bullet symbols.
        - Do not invent data.
        - Use only the provided report data.
        - Do not mention internal IDs unless needed.
        - Keep the response professional, concise, and practical.
        - If there is not enough data, say that clearly in the summary.
        - Avoid blaming individuals or teams.
        - Focus on patterns, risks, and actionable improvements.

        Report-specific focus:
        {{GetReportSpecificInstructions(reportType)}}

        Required JSON schema:
        {
          "summary": "string",
          "keyFindings": [
            {
              "title": "string",
              "description": "string"
            }
          ],
          "risks": [
            {
              "title": "string",
              "description": "string"
            }
          ],
          "recommendations": [
            {
              "title": "string",
              "description": "string"
            }
          ]
        }

        Section rules:
        - summary must be one clear paragraph.
        - keyFindings should contain 2 to 4 items.
        - risks should contain 1 to 3 items.
        - recommendations should contain 2 to 4 items.
        - Each item title must be short.
        - Each item description must be one or two sentences.
        - Use empty arrays only if the report data does not support that section.

        Report data:
        {{reportJson}}
        """;
    }

    private static ReportAiInsightDto ParseInsight(
        ReportType reportType,
        string aiResponse)
    {
        var json = ExtractJson(aiResponse);

        try
        {
            var insight = JsonSerializer.Deserialize<ReportAiInsightDto>(
                json,
                JsonOptions);

            if (insight is null)
            {
                return BuildFallbackInsight(
                    reportType,
                    "The AI response could not be parsed into a report insight.");
            }

            NormalizeInsight(insight);

            return insight;
        }
        catch (JsonException)
        {
            return BuildFallbackInsight(
                reportType,
                "The AI response was not in the expected structured format.");
        }
    }

    private static string ExtractJson(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
            return "{}";

        var trimmed = aiResponse.Trim();

        if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
            return trimmed;

        var codeBlockMatch = Regex.Match(
            trimmed,
            @"```(?:json)?\s*(\{[\s\S]*?\})\s*```",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        if (codeBlockMatch.Success)
            return codeBlockMatch.Groups[1].Value.Trim();

        var jsonMatch = Regex.Match(
            trimmed,
            @"\{[\s\S]*\}",
            RegexOptions.Multiline);

        return jsonMatch.Success
            ? jsonMatch.Value.Trim()
            : "{}";
    }

    private static void NormalizeInsight(ReportAiInsightDto insight)
    {
        insight.Summary = CleanText(insight.Summary);

        insight.KeyFindings = NormalizeItems(insight.KeyFindings);
        insight.Risks = NormalizeItems(insight.Risks);
        insight.Recommendations = NormalizeItems(insight.Recommendations);
    }

    private static List<ReportAiInsightItemDto> NormalizeItems(
        List<ReportAiInsightItemDto>? items)
    {
        return items?
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.Title) ||
                !string.IsNullOrWhiteSpace(item.Description))
            .Select(item => new ReportAiInsightItemDto
            {
                Title = CleanText(item.Title),
                Description = CleanText(item.Description)
            })
            .ToList() ?? [];
    }

    private static string CleanText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value
            .Replace("**", string.Empty)
            .Replace("*", string.Empty)
            .Replace("#", string.Empty)
            .Trim();
    }

    private static ReportAiInsightDto BuildFallbackInsight(
        ReportType reportType,
        string message)
    {
        return new ReportAiInsightDto
        {
            ReportType = reportType,
            Summary = message,
            KeyFindings = [],
            Risks = [],
            Recommendations = [],
            GeneratedAt = DateTime.UtcNow
        };
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