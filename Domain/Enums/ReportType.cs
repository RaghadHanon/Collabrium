namespace Domain.Models;

public enum ReportType
{
    TenantInfo = 0,
    TeamPerformance = 1,
    UserActivity = 2,
    SprintBurndown = 3,
    WorkItemDistribution = 4,
    ProjectProgress = 5,
    SprintProgress = 6,
    Velocity = 7,
    CycleTime = 8,
    BugSeverityDistribution = 9,
    DeveloperProductivity = 10,
    TeamsProductivity = 11,
    BlockedWorkItems = 12,
    OverdueWorkItems = 13,
    ProjectHealth = 14,
}