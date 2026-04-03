using Application.Features.Reports.DTOs;
using Application.Features.Reports.DTOs.Summaries;
using Application.Interfaces.ReadModels;
using AutoMapper;
using Domain.Enums;
using Domain.Models;
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
            UsersSummary = usersSummary,    
            TeamsSummary = teamsSummary,
            ProjectsSummary = projectsSummary,
            SprintsSummary = sprintsSummary,
            WorkItemsSummary = workItemsSummary
        };
    }

    public async Task<TeamsSummary> GenerateTeamsSummary(string tenantId)
    {
        var teamsSnapshot = await _dbContext.Teams
            .AsNoTracking()
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

    public async Task<ProjectsSummary> GenerateProjectsSummary(string tenantId)
    {
        var projectsSnapshot = await _dbContext.Projects
            .AsNoTracking()
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

    public async Task<SprintsSummary> GenerateSprintsSummary(string tenantId)
    {
        var sprintsSnapshot = await _dbContext.Sprints
            .AsNoTracking()
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

    public async Task<SprintsSummary> GenerateProjectSprintsSummary(string tenantId, string projectId)
    {
        var sprintsSnapshot = await _dbContext.Sprints
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.ProjectId == projectId)
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

    public async Task<WorkItemsSummary> GenerateWorkItemsSummary(string tenantId)
    {
        var workItemsSnapshot = await _dbContext.WorkItems
            .AsNoTracking()
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

    public async Task<UsersSummary> GenerateUsersSummary(string tenantId)
    {
        var usersSnapshot = await _dbContext.Users
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .Select(p => p.Id)
            .ToListAsync();

        return new UsersSummary
        {
            TotalUsersCount = usersSnapshot.Count
        };
    }

    public async Task<CycleTimeReport> GenerateCycleTimeReportAsync(string tenantId, string projectId, DateTime from, DateTime to, string? sprintId = null)
    {
        var projectKey = await _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId && p.TenantId == tenantId)
            .Select(p => p.Key )
            .FirstOrDefaultAsync();

        var query = _dbContext.WorkItems
            .AsNoTracking()
            .Where(wi => wi.ProjectId == projectId
                         && wi.Project.TenantId == tenantId
                         && wi.EndDate.HasValue
                         && wi.EndDate.Value >= from
                         && wi.EndDate.Value <= to
                         && wi.Status == WorkItemStatus.Done)
            .AsQueryable();

        if(!string.IsNullOrEmpty(sprintId))
            query = query.Where(wi => wi.SprintId == sprintId);

        var workItemsSnapshot = await query
            .Select(wi => new { 
                        Id = wi.Id,
                        Code = wi.Code,
                        CreatedAt = wi.CreatedAt,
                        EndDate = wi.EndDate 
            })
            .ToListAsync();

        var items = workItemsSnapshot.Select(wi => new WorkItemCycleTime
        {
            WorkItemId = wi.Id,
            Code = wi.Code,
            EndDate = wi.EndDate!.Value,
            CycleTimeDays = (wi.EndDate!.Value - wi.CreatedAt).Days
        }).ToList();

        return new CycleTimeReport
        {
            ProjectId = projectId,
            ProjectKey = projectKey!,
            GeneratedAt = DateTime.UtcNow,
            AverageCycleTimeDays = items.Any() ? items.Average(i => i.CycleTimeDays) : 0,
            MedianCycleTimeDays = items.Any() ? items.OrderBy(i => i.CycleTimeDays).ElementAt(items.Count / 2).CycleTimeDays : 0,
            MinCycleTimeDays = items.Any() ? items.Min(i => i.CycleTimeDays) : 0,
            MaxCycleTimeDays = items.Any() ? items.Max(i => i.CycleTimeDays) : 0,
            TotalItemsAnalyzed = items.Count,
            Items = items
        };
    }

    public async Task<SprintProgressDto> GetSprintProgress(string tenantId, string projectId, string sprintId)
    {
        var workItems = await _dbContext.WorkItems
            .AsNoTracking()
            .Where(wi => wi.SprintId == sprintId && wi.Project.TenantId == tenantId && wi.ProjectId == projectId)
            .Select(wi => new { wi.Id, wi.Status })
            .ToListAsync();

        var totalWorkItems = workItems.Count;
        var completedWorkItems = workItems.Count(wi => wi.Status == WorkItemStatus.Done);

        var userStories = await _dbContext.UserStories
            .AsNoTracking()
            .Where(wi => wi.SprintId == sprintId && wi.Project.TenantId == tenantId && wi.ProjectId == projectId)
            .Select(wi => new { wi.Id, wi.Status, wi.StoryPoints })
            .ToListAsync();

        var totalStoryPoints = userStories.Sum(us => us.StoryPoints);
        var completedStoryPoints = userStories.Where(us => us.Status == WorkItemStatus.Done).Sum(wi => wi.StoryPoints);

        return new SprintProgressDto
        {
            SprintId = sprintId,
            TotalWorkItems = totalWorkItems,
            CompletedWorkItems = completedWorkItems,
            GeneralProgressPercentage = totalWorkItems > 0 ? (double)completedWorkItems / totalWorkItems * 100 : 0,
            UserStoryProgressPercentage = totalStoryPoints > 0 ? (double)completedStoryPoints / totalStoryPoints * 100 : 0
        };
    }
}