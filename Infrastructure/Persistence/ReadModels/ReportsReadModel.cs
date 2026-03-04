using Application.Features.Reports.DTOs;
using Application.Features.Reports.DTOs.Summaries;
using Application.Interfaces.ReadModels;
using AutoMapper;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public class ReportsReadModel : IReportsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public ReportsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<TenantInfoReport> GenerateTenantInfoReportAsync(string tenantId)
    {
        var teamsSummary = await GenerateTeamsSummary(tenantId);
        var projectsSummary = await GenerateProjectsSummary(tenantId);
        var sprintsSummary = await GenerateSprintsSummary(tenantId);
        var workItemsSummary = await GenerateWorkItemsSummary(tenantId);
        var usersSummary = await GenerateUsersSummary(tenantId);

        return new TenantInfoReport
        {
            TenantId = tenantId,
            GeneratedAt = DateTime.UtcNow,
            TeamsSummary = teamsSummary,
            ProjectsSummary = projectsSummary,
            SprintsSummary = sprintsSummary,
            WorkItemsSummary = workItemsSummary
        };
    }

    private async Task<TeamsSummary> GenerateTeamsSummary(string tenantId)
    {
        var teamsSnapshot = await _dbContext.Teams
            .Where(t => t.TenantId == tenantId)
            .Select(t => new { t.Id, t.IsActive })
            .ToListAsync();

        return new TeamsSummary
        {
            TotalTeamsCount = teamsSnapshot.Count,
            ActiveTeamsCount = teamsSnapshot.Count(t => t.IsActive),
            InActiveTeamsCount = teamsSnapshot.Count(t => !t.IsActive)
        };
    }

    private async Task<ProjectsSummary> GenerateProjectsSummary(string tenantId)
    {
        var projectsSnapshot = await _dbContext.Projects
            .Where(t => t.TenantId == tenantId)
            .Select(p => new { p.Id, p.Status })
            .ToListAsync();

        var groupedByStatus = projectsSnapshot
            .GroupBy(p => p.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return new ProjectsSummary
        {
            TotalProjectsCount = projectsSnapshot.Count,
            NotStartedProjectsCount = groupedByStatus.GetValueOrDefault(ProjectStatus.NotStarted),
            InProgressProjectsCount = groupedByStatus.GetValueOrDefault(ProjectStatus.InProgress),
            CompletedProjectsCount = groupedByStatus.GetValueOrDefault(ProjectStatus.Completed),
            OnHoldProjectsCount = groupedByStatus.GetValueOrDefault(ProjectStatus.OnHold),
            CancelledProjectsCount = groupedByStatus.GetValueOrDefault(ProjectStatus.Cancelled),
        };
    }

    private async Task<SprintsSummary> GenerateSprintsSummary(string tenantId)
    {
        var sprintsSnapshot = await _dbContext.Sprints
            .Where(t => t.TenantId == tenantId)
            .Select(p => new { p.Id, p.Status })
            .ToListAsync();

        var groupedByStatus = sprintsSnapshot
            .GroupBy(p => p.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return new SprintsSummary
        {
            TotalSprintsCount = sprintsSnapshot.Count,
            PlannedSprintsCount = groupedByStatus.GetValueOrDefault(SprintStatus.Planned),
            ActiveSprintsCount = groupedByStatus.GetValueOrDefault(SprintStatus.Active),
            CompletedSprintsCount = groupedByStatus.GetValueOrDefault(SprintStatus.Completed),
            CancelledSprintsCount = groupedByStatus.GetValueOrDefault(SprintStatus.Cancelled),
        };
    }

    private async Task<WorkItemsSummary> GenerateWorkItemsSummary(string tenantId)
    {
        var workItemsSnapshot = await _dbContext.WorkItems
            .Where(t => t.Project.TenantId == tenantId)
            .Select(p => new { p.Id, p.Type })
            .ToListAsync();

        var groupedByStatus = workItemsSnapshot
            .GroupBy(p => p.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        return new WorkItemsSummary
        {
            TotalTasksCount = workItemsSnapshot.Count,
            EpicsCount = groupedByStatus.GetValueOrDefault(WorkItemType.Epic),
            UserStoriesCount = groupedByStatus.GetValueOrDefault(WorkItemType.UserStory),
            TaskItemCount = groupedByStatus.GetValueOrDefault(WorkItemType.TaskItem),
            BugsCount = groupedByStatus.GetValueOrDefault(WorkItemType.Bug),
            TestCasesCount = groupedByStatus.GetValueOrDefault(WorkItemType.TestCase)
        };
    }

    private async Task<UsersSummary> GenerateUsersSummary(string tenantId)
    {
        var usersSnapshot = await _dbContext.Users
            .Where(t => t.TenantId == tenantId)
            .Select(p => p.Id)
            .ToListAsync();

        return new UsersSummary
        {
            TotalUsersCount = usersSnapshot.Count
        };
    }
}