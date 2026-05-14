using Application.Features.Reports.DTOs;
using Application.Features.Reports.DTOs.Shared;
using Application.Features.Reports.DTOs.Summaries;
using Application.Features.Tenants.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public class ReportsReadModel : IReportsReadModel
{
    private const int DefaultVelocitySprintsCount = 10;
    private const int DefaultMaxReportItems = 100;

    private const string HealthStatusHealthy = "Healthy";
    private const string HealthStatusAtRisk = "At Risk";
    private const string HealthStatusCritical = "Critical";

    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public ReportsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<TenantInfoReport> GenerateTenantInfoReportAsync(string tenantId)
    {
        var generatedAt = DateTime.UtcNow;
        var tenant = await GetRequiredTenantAsync(tenantId);

        return new TenantInfoReport
        {
            TenantId = tenantId,
            Tenant = tenant,
            GeneratedAt = generatedAt,
            UsersSummary = await GenerateUsersSummary(tenantId),
            TeamsSummary = await GenerateTeamsSummary(tenantId),
            ProjectsSummary = await GenerateProjectsSummary(tenantId),
            SprintsSummary = await GenerateSprintsSummary(tenantId),
            WorkItemsSummary = await GenerateWorkItemsSummary(tenantId)
        };
    }

    public async Task<TeamsSummary> GenerateTeamsSummary(string tenantId)
    {
        var teams = await _dbContext.Teams
            .AsNoTracking()
            .Where(team => team.TenantId == tenantId)
            .Select(team => new { team.IsActive })
            .ToListAsync();

        return new TeamsSummary
        {
            TotalTeamsCount = teams.Count,
            ActiveTeamsCount = teams.Count(team => team.IsActive),
            InActiveTeamsCount = teams.Count(team => !team.IsActive)
        };
    }

    public async Task<ProjectsSummary> GenerateProjectsSummary(string tenantId)
    {
        var statuses = await _dbContext.Projects
            .AsNoTracking()
            .Where(project => project.TenantId == tenantId)
            .Select(project => project.Status)
            .ToListAsync();

        return BuildProjectsSummary(statuses);
    }

    public async Task<SprintsSummary> GenerateSprintsSummary(string tenantId)
    {
        var statuses = await _dbContext.Sprints
            .AsNoTracking()
            .Where(sprint => sprint.TenantId == tenantId)
            .Select(sprint => sprint.Status)
            .ToListAsync();

        return BuildSprintsSummary(statuses);
    }

    public async Task<SprintsSummary> GenerateProjectSprintsSummary(string tenantId, string projectId)
    {
        await GetRequiredProjectAsync(tenantId, projectId);

        var statuses = await _dbContext.Sprints
            .AsNoTracking()
            .Where(sprint => sprint.TenantId == tenantId && sprint.ProjectId == projectId)
            .Select(sprint => sprint.Status)
            .ToListAsync();

        return BuildSprintsSummary(statuses);
    }

    public async Task<WorkItemsSummary> GenerateWorkItemsSummary(string tenantId)
    {
        var types = await QueryTenantWorkItems(tenantId)
            .Select(workItem => workItem.Type)
            .ToListAsync();

        var countsByType = CountBy(types);

        return new WorkItemsSummary
        {
            TotalWorkItemsCount = types.Count,
            EpicsCount = countsByType.GetValueOrDefault(WorkItemType.Epic),
            UserStoriesCount = countsByType.GetValueOrDefault(WorkItemType.UserStory),
            TaskItemCount = countsByType.GetValueOrDefault(WorkItemType.TaskItem),
            BugsCount = countsByType.GetValueOrDefault(WorkItemType.Bug),
            TestCasesCount = countsByType.GetValueOrDefault(WorkItemType.TestCase)
        };
    }

    public async Task<UsersSummary> GenerateUsersSummary(string tenantId)
    {
        var usersCount = await _dbContext.Users
            .AsNoTracking()
            .CountAsync(user => user.TenantId == tenantId);

        return new UsersSummary
        {
            TotalUsersCount = usersCount
        };
    }

    public async Task<CycleTimeReport> GenerateCycleTimeReportAsync(
        string tenantId,
        string projectId,
        DateTime from,
        DateTime to,
        string? sprintId = null)
    {
        ValidateDateRange(from, to);

        var generatedAt = DateTime.UtcNow;
        var project = await GetRequiredProjectAsync(tenantId, projectId);

        var query = QueryTenantWorkItems(tenantId)
            .Where(workItem =>
                workItem.ProjectId == projectId &&
                workItem.EndDate.HasValue &&
                workItem.EndDate.Value >= from &&
                workItem.EndDate.Value <= to &&
                workItem.Status == WorkItemStatus.Done);

        if (!string.IsNullOrWhiteSpace(sprintId))
            query = query.Where(workItem => workItem.SprintId == sprintId);

        var workItems = await query
            .Select(workItem => new WorkItemCycleTimeProjection
            {
                Id = workItem.Id,
                Code = workItem.Code,
                CreatedAt = workItem.CreatedAt,
                StartDate = workItem.StartDate,
                EndDate = workItem.EndDate!.Value
            })
            .ToListAsync();

        var items = workItems
            .Select(ToWorkItemCycleTime)
            .ToList();

        return new CycleTimeReport
        {
            ProjectId = project.Id,
            ProjectKey = project.Key,
            From = from,
            To = to,
            GeneratedAt = generatedAt,
            AverageCycleTimeDays = AverageOrZero(items, item => item.CycleTimeDays),
            MedianCycleTimeDays = CalculateMedianCycleTimeDays(items),
            MinCycleTimeDays = items.Count > 0 ? items.Min(item => item.CycleTimeDays) : 0,
            MaxCycleTimeDays = items.Count > 0 ? items.Max(item => item.CycleTimeDays) : 0,
            TotalItemsAnalyzed = items.Count,
            Items = items
        };
    }

    public async Task<SprintProgressDto> GenerateSprintProgressReport(
        string tenantId,
        string projectId,
        string sprintId)
    {
        await GetRequiredProjectAsync(tenantId, projectId);

        var sprint = await _dbContext.Sprints
            .AsNoTracking()
            .Where(s => s.Id == sprintId && s.ProjectId == projectId && s.TenantId == tenantId)
            .Select(s => new
            {
                s.Id,
                s.Code,
                ProjectKey = s.Project.Key,
                ProjectName = s.Project.Name
            })
            .FirstOrDefaultAsync();

        if (sprint is null)
            throw new InvalidOperationException($"Sprint with id {sprintId} was not found for project {projectId} and tenant {tenantId}.");

        var workItems = await QueryTenantWorkItems(tenantId)
            .Where(workItem => workItem.ProjectId == projectId && workItem.SprintId == sprintId)
            .Select(workItem => workItem.Status)
            .ToListAsync();

        var userStories = await _dbContext.UserStories
            .AsNoTracking()
            .Where(userStory =>
                userStory.Project.TenantId == tenantId &&
                userStory.ProjectId == projectId &&
                userStory.SprintId == sprintId)
            .Select(userStory => new
            {
                userStory.Status,
                userStory.StoryPoints
            })
            .ToListAsync();

        var totalWorkItems = workItems.Count;
        var completedWorkItems = workItems.Count(IsDone);

        var totalStoryPoints = userStories.Sum(userStory => userStory.StoryPoints);
        var completedStoryPoints = userStories
            .Where(userStory => IsDone(userStory.Status))
            .Sum(userStory => userStory.StoryPoints);

        return new SprintProgressDto
        {
            SprintId = sprint.Id,
            SprintCode = sprint.Code,
            ProjectKey = sprint.ProjectKey,
            ProjectName = sprint.ProjectName,
            TotalWorkItems = totalWorkItems,
            CompletedWorkItems = completedWorkItems,
            GeneralProgressPercentage = CalculatePercentage(completedWorkItems, totalWorkItems),
            UserStoryProgressPercentage = CalculatePercentage(completedStoryPoints, totalStoryPoints)
        };
    }

    public async Task<VelocityReportDto> GenerateVelocityReport(string tenantId, string projectId)
    {
        var project = await GetRequiredProjectAsync(tenantId, projectId);

        var sprintRows = await _dbContext.UserStories
            .AsNoTracking()
            .Where(userStory =>
                userStory.Project.TenantId == tenantId &&
                userStory.ProjectId == projectId &&
                userStory.SprintId != null &&
                userStory.Sprint!.Status == SprintStatus.Completed)
            .GroupBy(userStory => new
            {
                userStory.SprintId,
                userStory.Sprint!.Code,
                userStory.Sprint.StartDate
            })
            .Select(group => new SprintVelocityProjection
            {
                SprintId = group.Key.SprintId!,
                SprintCode = group.Key.Code,
                StartDate = group.Key.StartDate,
                TotalStoryPoints = group.Sum(userStory => userStory.StoryPoints),
                CompletedStoryPoints = group
                    .Where(userStory => userStory.Status == WorkItemStatus.Done)
                    .Sum(userStory => userStory.StoryPoints)
            })
            .OrderByDescending(sprint => sprint.StartDate)
            .Take(DefaultVelocitySprintsCount)
            .ToListAsync();

        var sprintVelocities = sprintRows
            .OrderBy(sprint => sprint.StartDate)
            .Select(ToSprintVelocityDto)
            .ToList();

        return new VelocityReportDto
        {
            ProjectId = project.Id,
            ProjectKey = project.Key,
            ProjectName = project.Name,
            TotalStoryPoints = sprintVelocities.Sum(sprint => sprint.TotalStoryPoints),
            CompletedStoryPoints = sprintVelocities.Sum(sprint => sprint.CompletedStoryPoints),
            AvgVelocityPercentage = AverageOrZero(sprintVelocities, sprint => sprint.VelocityPercentage),
            SprintVelocities = sprintVelocities
        };
    }

    public async Task<BugSeverityDistributionReportDto> GenerateBugSeverityDistributionReportAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        string? projectId = null)
    {
        ValidateDateRange(from, to);

        var generatedAt = DateTime.UtcNow;
        var project = await GetOptionalProjectAsync(tenantId, projectId);

        var query = QueryTenantWorkItems(tenantId)
            .Where(workItem =>
                workItem.Type == WorkItemType.Bug &&
                workItem.CreatedAt >= from &&
                workItem.CreatedAt <= to);

        query = ApplyProjectFilter(query, projectId);

        var priorities = await query
            .Select(workItem => workItem.Priority)
            .ToListAsync();

        var totalBugs = priorities.Count;
        var countsByPriority = CountBy(priorities);

        var lowCount = countsByPriority.GetValueOrDefault(WorkItemPriority.Low);
        var mediumCount = countsByPriority.GetValueOrDefault(WorkItemPriority.Medium);
        var highCount = countsByPriority.GetValueOrDefault(WorkItemPriority.High);
        var criticalCount = countsByPriority.GetValueOrDefault(WorkItemPriority.Critical);

        return new BugSeverityDistributionReportDto
        {
            TenantId = tenantId,
            ProjectId = projectId,
            ProjectKey = project?.Key,
            ProjectName = project?.Name,
            From = from,
            To = to,
            GeneratedAt = generatedAt,
            TotalBugs = totalBugs,
            LowBugsCount = lowCount,
            MediumBugsCount = mediumCount,
            HighBugsCount = highCount,
            CriticalBugsCount = criticalCount,
            LowPercentage = CalculatePercentage(lowCount, totalBugs),
            MediumPercentage = CalculatePercentage(mediumCount, totalBugs),
            HighPercentage = CalculatePercentage(highCount, totalBugs),
            CriticalPercentage = CalculatePercentage(criticalCount, totalBugs),
            Items = BuildBugSeverityDistributionItems(countsByPriority, totalBugs)
        };
    }

    public async Task<DeveloperProductivityReportDto> GenerateDeveloperProductivityReportAsync(
    string tenantId,
    DateTime from,
    DateTime to,
    string? projectId = null,
    string? teamId = null,
    string? userId = null)
    {
        ValidateDateRange(from, to);

        var generatedAt = DateTime.UtcNow;
        var now = DateTime.UtcNow;
        var project = await GetOptionalProjectAsync(tenantId, projectId);

        var users = await GetDeveloperCandidatesAsync(tenantId, teamId, userId);
        var userIds = users.Select(user => user.Id).ToList();

        var workItemsQuery = QueryTenantWorkItems(tenantId);
        workItemsQuery = ApplyProjectFilter(workItemsQuery, projectId);
        workItemsQuery = ApplyTeamFilter(workItemsQuery, teamId);
        workItemsQuery = ApplyOverlappingDateRange(workItemsQuery, from, to);

        workItemsQuery = workItemsQuery.Where(workItem =>
            userIds.Contains(workItem.CreatedByUserId) ||
            (workItem.AssignedToUserId != null && userIds.Contains(workItem.AssignedToUserId)));

        var workItems = await SelectWorkItemMetrics(workItemsQuery).ToListAsync();

        var developers = users
            .Select(user => BuildDeveloperProductivityItem(user, workItems, from, to, now))
            .Where(developer => !string.IsNullOrWhiteSpace(userId) || HasDeveloperActivity(developer))
            .OrderByDescending(developer => developer.CompletedEffortPoints)
            .ThenByDescending(developer => developer.CompletedWorkItemsCount)
            .ToList();

        return new DeveloperProductivityReportDto
        {
            TenantId = tenantId,
            ProjectId = projectId,
            ProjectKey = project?.Key,
            ProjectName = project?.Name,
            TeamId = teamId,
            UserId = userId,

            From = from,
            To = to,
            GeneratedAt = generatedAt,

            DevelopersCount = developers.Count,

            TotalAssignedWorkItems = developers.Sum(developer => developer.AssignedWorkItemsCount),
            TotalCompletedWorkItems = developers.Sum(developer => developer.CompletedWorkItemsCount),
            TotalBlockedWorkItems = developers.Sum(developer => developer.BlockedWorkItemsCount),
            TotalOverdueWorkItems = developers.Sum(developer => developer.OverdueWorkItemsCount),
            TotalCompletedEffortPoints = developers.Sum(developer => developer.CompletedEffortPoints),

            AverageCompletionPercentage = AverageOrZero(developers, developer => developer.CompletionPercentage),
            AverageCycleTimeDays = AverageForItemsWithCompletedWork(developers),

            Developers = developers
        };
    }

    public async Task<TeamsProductivityReportDto> GenerateTeamsProductivityReportAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        string? projectId = null)
    {
        ValidateDateRange(from, to);

        var generatedAt = DateTime.UtcNow;
        var now = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(projectId))
            await GetRequiredProjectAsync(tenantId, projectId);

        var teams = await GetActiveTeamsAsync(tenantId, projectId);
        var teamIds = teams.Select(team => team.Id).ToList();

        var workItemsQuery = QueryTenantWorkItems(tenantId)
            .Where(workItem =>
                workItem.Project.TeamId != null &&
                teamIds.Contains(workItem.Project.TeamId));

        workItemsQuery = ApplyProjectFilter(workItemsQuery, projectId);
        workItemsQuery = ApplyOverlappingDateRange(workItemsQuery, from, to);

        var workItems = await SelectWorkItemMetrics(workItemsQuery).ToListAsync();

        var teamItems = teams
            .Select(team => BuildTeamProductivityItem(team, workItems, from, to, now))
            .Where(HasTeamActivity)
            .OrderByDescending(team => team.CompletedEffortPoints)
            .ThenByDescending(team => team.CompletedWorkItemsCount)
            .ToList();

        return new TeamsProductivityReportDto
        {
            TenantId = tenantId,
            ProjectId = projectId,
            From = from,
            To = to,
            GeneratedAt = generatedAt,
            TeamsCount = teamItems.Count,
            TotalAssignedWorkItems = teamItems.Sum(team => team.AssignedWorkItemsCount),
            TotalCompletedWorkItems = teamItems.Sum(team => team.CompletedWorkItemsCount),
            TotalBlockedWorkItems = teamItems.Sum(team => team.BlockedWorkItemsCount),
            TotalOverdueWorkItems = teamItems.Sum(team => team.OverdueWorkItemsCount),
            TotalCompletedEffortPoints = teamItems.Sum(team => team.CompletedEffortPoints),
            AverageCompletionPercentage = AverageOrZero(teamItems, team => team.CompletionPercentage),
            AverageCycleTimeDays = AverageForTeamsWithCompletedWork(teamItems),
            Teams = teamItems
        };
    }

    public async Task<BlockedWorkItemsReportDto> GenerateBlockedWorkItemsReportAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        string? projectId = null,
        string? teamId = null,
        string? assignedToUserId = null,
        int maxItems = DefaultMaxReportItems)
    {
        ValidateDateRange(from, to);

        var generatedAt = DateTime.UtcNow;
        var take = NormalizeMaxItems(maxItems);

        var query = QueryTenantWorkItems(tenantId)
            .Where(workItem => workItem.IsBlocked && workItem.Status != WorkItemStatus.Done);

        query = ApplyWorkItemScopeFilters(query, projectId, teamId, assignedToUserId);
        query = ApplyOverlappingDateRange(query, from, to);

        var workItems = await SelectWorkItemDetails(query).ToListAsync();
        var total = workItems.Count;

        var items = workItems
            .Select(workItem => ToBlockedWorkItemDto(workItem, generatedAt))
            .OrderByDescending(item => item.Priority)
            .ThenByDescending(item => item.BlockedAgeDays)
            .Take(take)
            .ToList();

        return new BlockedWorkItemsReportDto
        {
            TenantId = tenantId,
            ProjectId = projectId,
            TeamId = teamId,
            AssignedToUserId = assignedToUserId,
            From = from,
            To = to,
            GeneratedAt = generatedAt,
            TotalBlockedItems = total,
            CriticalBlockedItems = workItems.Count(workItem => workItem.Priority == WorkItemPriority.Critical),
            HighBlockedItems = workItems.Count(workItem => workItem.Priority == WorkItemPriority.High),
            TotalBlockedEffortPoints = workItems.Sum(workItem => workItem.EffortPoints),
            AverageBlockedAgeDays = AverageOrZero(workItems, workItem => CalculateNonNegativeDays(workItem.CreatedAt, generatedAt)),
            ByPriority = BuildPriorityDistribution(workItems.Select(workItem => workItem.Priority), total),
            ByStatus = BuildStatusDistribution(workItems.Select(workItem => workItem.Status), total),
            Items = items
        };
    }

    public async Task<OverdueWorkItemsReportDto> GenerateOverdueWorkItemsReportAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        string? projectId = null,
        string? teamId = null,
        string? assignedToUserId = null,
        int maxItems = DefaultMaxReportItems)
    {
        ValidateDateRange(from, to);

        var generatedAt = DateTime.UtcNow;
        var take = NormalizeMaxItems(maxItems);

        var query = QueryTenantWorkItems(tenantId)
            .Where(workItem =>
                workItem.Status != WorkItemStatus.Done &&
                workItem.EndDate.HasValue &&
                workItem.EndDate.Value < generatedAt &&
                workItem.EndDate.Value >= from &&
                workItem.EndDate.Value <= to);

        query = ApplyWorkItemScopeFilters(query, projectId, teamId, assignedToUserId);

        var workItems = await SelectWorkItemDetails(query).ToListAsync();
        var total = workItems.Count;

        var items = workItems
            .Select(workItem => ToOverdueWorkItemDto(workItem, generatedAt))
            .OrderByDescending(item => item.Priority)
            .ThenByDescending(item => item.DaysOverdue)
            .Take(take)
            .ToList();

        return new OverdueWorkItemsReportDto
        {
            TenantId = tenantId,
            ProjectId = projectId,
            TeamId = teamId,
            AssignedToUserId = assignedToUserId,
            From = from,
            To = to,
            GeneratedAt = generatedAt,
            TotalOverdueItems = total,
            CriticalOverdueItems = workItems.Count(workItem => workItem.Priority == WorkItemPriority.Critical),
            HighOverdueItems = workItems.Count(workItem => workItem.Priority == WorkItemPriority.High),
            TotalOverdueEffortPoints = workItems.Sum(workItem => workItem.EffortPoints),
            AverageDaysOverdue = AverageOrZero(workItems, workItem =>
                workItem.EndDate.HasValue
                    ? CalculateNonNegativeDays(workItem.EndDate.Value, generatedAt)
                    : 0),
            ByPriority = BuildPriorityDistribution(workItems.Select(workItem => workItem.Priority), total),
            ByStatus = BuildStatusDistribution(workItems.Select(workItem => workItem.Status), total),
            Items = items
        };
    }

    public async Task<ProjectHealthReportDto> GenerateProjectHealthReportAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        string? projectId = null)
    {
        ValidateDateRange(from, to);

        var generatedAt = DateTime.UtcNow;

        var projectsQuery = _dbContext.Projects
            .AsNoTracking()
            .Where(project => project.TenantId == tenantId);

        projectsQuery = ApplyProjectFilter(projectsQuery, projectId);

        var projects = await projectsQuery
            .Select(project => new ProjectHealthProjection
            {
                Id = project.Id,
                Key = project.Key,
                Name = project.Name,
                Status = project.Status,
                TeamId = project.TeamId,
                TeamName = project.Team != null ? project.Team.Name : null
            })
            .ToListAsync();

        if (projectId is not null && projects.Count == 0)
            throw new InvalidOperationException($"Project with id {projectId} was not found for tenant {tenantId}.");

        var projectIds = projects.Select(project => project.Id).ToList();

        var workItems = await SelectWorkItemMetrics(QueryTenantWorkItems(tenantId)
                .Where(workItem => projectIds.Contains(workItem.ProjectId))
                .Where(workItem =>
                    workItem.CreatedAt <= to &&
                    (!workItem.EndDate.HasValue || workItem.EndDate.Value >= from)))
            .ToListAsync();

        var projectHealthItems = projects
            .Select(project => BuildProjectHealthItem(project, workItems, generatedAt))
            .OrderBy(project => project.HealthScore)
            .ThenByDescending(project => project.CriticalBugs)
            .ThenByDescending(project => project.OverdueWorkItems)
            .ToList();

        return new ProjectHealthReportDto
        {
            TenantId = tenantId,
            ProjectId = projectId,
            From = from,
            To = to,
            GeneratedAt = generatedAt,
            ProjectsCount = projectHealthItems.Count,
            HealthyProjectsCount = projectHealthItems.Count(project => project.HealthStatus == HealthStatusHealthy),
            AtRiskProjectsCount = projectHealthItems.Count(project => project.HealthStatus == HealthStatusAtRisk),
            CriticalProjectsCount = projectHealthItems.Count(project => project.HealthStatus == HealthStatusCritical),
            AverageHealthScore = AverageOrZero(projectHealthItems, project => project.HealthScore),
            Projects = projectHealthItems
        };
    }

    private async Task<TenantDto> GetRequiredTenantAsync(string tenantId)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Id == tenantId)
            .ProjectTo<TenantDto>(_mapper.ConfigurationProvider, t => t.Name)
            .FirstOrDefaultAsync();

        return tenant ?? throw new InvalidOperationException($"Tenant with id {tenantId} not found.");
    }

    private async Task<ProjectLookup> GetRequiredProjectAsync(string tenantId, string projectId)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId && p.TenantId == tenantId)
            .Select(p => new ProjectLookup
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name
            })
            .FirstOrDefaultAsync();

        return project ?? throw new InvalidOperationException($"Project with id {projectId} was not found for tenant {tenantId}.");
    }

    private async Task<ProjectLookup?> GetOptionalProjectAsync(string tenantId, string? projectId)
    {
        return string.IsNullOrWhiteSpace(projectId)
            ? null
            : await GetRequiredProjectAsync(tenantId, projectId);
    }

    private IQueryable<WorkItemEntity> QueryTenantWorkItems(string tenantId)
    {
        return _dbContext.WorkItems
            .AsNoTracking()
            .Where(workItem => workItem.Project.TenantId == tenantId);
    }

    private async Task<List<UserProjection>> GetDeveloperCandidatesAsync(
        string tenantId,
        string? teamId,
        string? userId)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Where(user => user.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(teamId))
        {
            query = query.Where(user =>
                user.TeamMemberships.Any(member => member.TeamId == teamId));
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(user => user.Id == userId);
        }

        return await query
            .Select(user => new UserProjection
            {
                Id = user.Id,
                FullName = user.FullName,
                JobTitle = user.JobTitle,
                Department = user.Department
            })
            .ToListAsync();
    }

    private async Task<List<TeamProjection>> GetActiveTeamsAsync(string tenantId, string? projectId)
    {
        var query = _dbContext.Teams
            .AsNoTracking()
            .Where(team => team.TenantId == tenantId && team.IsActive);

        if (!string.IsNullOrWhiteSpace(projectId))
            query = query.Where(team => team.Projects.Any(project => project.Id == projectId));

        return await query
            .Select(team => new TeamProjection
            {
                Id = team.Id,
                Name = team.Name,
                TeamLeadId = team.TeamLeadId,
                TeamLeadName = team.TeamLead.FullName,
                Department = team.Department,
                Capacity = team.Capacity,
                MembersCount = team.Members.Count,
                ActiveProjectsCount = team.Projects.Count(project => project.Status == ProjectStatus.InProgress)
            })
            .ToListAsync();
    }

    private static IQueryable<ProjectEntity> ApplyProjectFilter(IQueryable<ProjectEntity> query, string? projectId)
    {
        return string.IsNullOrWhiteSpace(projectId)
            ? query
            : query.Where(project => project.Id == projectId);
    }

    private static IQueryable<WorkItemEntity> ApplyProjectFilter(IQueryable<WorkItemEntity> query, string? projectId)
    {
        return string.IsNullOrWhiteSpace(projectId)
            ? query
            : query.Where(workItem => workItem.ProjectId == projectId);
    }

    private static IQueryable<WorkItemEntity> ApplyTeamFilter(IQueryable<WorkItemEntity> query, string? teamId)
    {
        return string.IsNullOrWhiteSpace(teamId)
            ? query
            : query.Where(workItem => workItem.Project.TeamId == teamId);
    }

    private static IQueryable<WorkItemEntity> ApplyAssignedToUserFilter(IQueryable<WorkItemEntity> query, string? assignedToUserId)
    {
        return string.IsNullOrWhiteSpace(assignedToUserId)
            ? query
            : query.Where(workItem => workItem.AssignedToUserId == assignedToUserId);
    }

    private static IQueryable<WorkItemEntity> ApplyWorkItemScopeFilters(
        IQueryable<WorkItemEntity> query,
        string? projectId,
        string? teamId,
        string? assignedToUserId)
    {
        query = ApplyProjectFilter(query, projectId);
        query = ApplyTeamFilter(query, teamId);
        query = ApplyAssignedToUserFilter(query, assignedToUserId);

        return query;
    }

    private static IQueryable<WorkItemEntity> ApplyOverlappingDateRange(
        IQueryable<WorkItemEntity> query,
        DateTime from,
        DateTime to)
    {
        return query.Where(workItem =>
            workItem.CreatedAt <= to &&
            (!workItem.EndDate.HasValue || workItem.EndDate.Value >= from));
    }

    private static IQueryable<WorkItemMetricsProjection> SelectWorkItemMetrics(IQueryable<WorkItemEntity> query)
    {
        return query.Select(workItem => new WorkItemMetricsProjection
        {
            Id = workItem.Id,
            ProjectId = workItem.ProjectId,
            TeamId = workItem.Project.TeamId,
            AssignedToUserId = workItem.AssignedToUserId,
            CreatedByUserId = workItem.CreatedByUserId,
            CreatedAt = workItem.CreatedAt,
            StartDate = workItem.StartDate,
            EndDate = workItem.EndDate,
            Type = workItem.Type,
            Status = workItem.Status,
            Priority = workItem.Priority,
            IsBlocked = workItem.IsBlocked,
            EffortPoints = workItem.EffortPoints
        });
    }

    private static IQueryable<WorkItemDetailsProjection> SelectWorkItemDetails(IQueryable<WorkItemEntity> query)
    {
        return query.Select(workItem => new WorkItemDetailsProjection
        {
            Id = workItem.Id,
            Code = workItem.Code,
            Title = workItem.Title,
            Type = workItem.Type,
            Status = workItem.Status,
            Priority = workItem.Priority,
            EffortPoints = workItem.EffortPoints,
            CreatedAt = workItem.CreatedAt,
            EndDate = workItem.EndDate,
            ProjectId = workItem.ProjectId,
            ProjectKey = workItem.Project.Key,
            ProjectName = workItem.Project.Name,
            AssignedToUserId = workItem.AssignedToUserId,
            AssignedToUserName = workItem.AssignedToUser != null
                ? workItem.AssignedToUser.FullName
                : null
        });
    }

    private static ProjectsSummary BuildProjectsSummary(IReadOnlyCollection<ProjectStatus> statuses)
    {
        var countsByStatus = CountBy(statuses);

        return new ProjectsSummary
        {
            TotalProjectsCount = statuses.Count,
            NotStartedProjectsCount = countsByStatus.GetValueOrDefault(ProjectStatus.NotStarted),
            InProgressProjectsCount = countsByStatus.GetValueOrDefault(ProjectStatus.InProgress),
            CompletedProjectsCount = countsByStatus.GetValueOrDefault(ProjectStatus.Completed),
            OnHoldProjectsCount = countsByStatus.GetValueOrDefault(ProjectStatus.OnHold),
            CancelledProjectsCount = countsByStatus.GetValueOrDefault(ProjectStatus.Cancelled)
        };
    }

    private static SprintsSummary BuildSprintsSummary(IReadOnlyCollection<SprintStatus> statuses)
    {
        var countsByStatus = CountBy(statuses);

        return new SprintsSummary
        {
            TotalSprintsCount = statuses.Count,
            PlannedSprintsCount = countsByStatus.GetValueOrDefault(SprintStatus.Planned),
            ActiveSprintsCount = countsByStatus.GetValueOrDefault(SprintStatus.Active),
            CompletedSprintsCount = countsByStatus.GetValueOrDefault(SprintStatus.Completed),
            CancelledSprintsCount = countsByStatus.GetValueOrDefault(SprintStatus.Cancelled)
        };
    }

    private static WorkItemCycleTime ToWorkItemCycleTime(WorkItemCycleTimeProjection snapshot)
    {
        var startDate = snapshot.StartDate ?? snapshot.CreatedAt;

        return new WorkItemCycleTime
        {
            WorkItemId = snapshot.Id,
            Code = snapshot.Code,
            EndDate = snapshot.EndDate,
            CycleTimeDays = CalculateNonNegativeDays(startDate, snapshot.EndDate)
        };
    }

    private static SprintVelocityDto ToSprintVelocityDto(SprintVelocityProjection snapshot)
    {
        return new SprintVelocityDto
        {
            SprintId = snapshot.SprintId,
            SprintCode = snapshot.SprintCode,
            TotalStoryPoints = snapshot.TotalStoryPoints,
            CompletedStoryPoints = snapshot.CompletedStoryPoints,
            VelocityPercentage = CalculatePercentage(snapshot.CompletedStoryPoints, snapshot.TotalStoryPoints)
        };
    }

    private static DeveloperProductivityItemDto BuildDeveloperProductivityItem(
        UserProjection user,
        IReadOnlyCollection<WorkItemMetricsProjection> workItems,
        DateTime from,
        DateTime to,
        DateTime now)
    {
        var assignedItems = workItems
            .Where(workItem => workItem.AssignedToUserId == user.Id)
            .ToList();

        var completedItems = assignedItems
            .Where(workItem => IsCompletedWithin(workItem, from, to))
            .ToList();

        var assignedEffortPoints = assignedItems.Sum(workItem => workItem.EffortPoints);
        var completedEffortPoints = completedItems.Sum(workItem => workItem.EffortPoints);

        return new DeveloperProductivityItemDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            JobTitle = user.JobTitle,
            Department = user.Department?.ToString(),
            AssignedWorkItemsCount = assignedItems.Count,
            CreatedWorkItemsCount = workItems.Count(workItem =>
                workItem.CreatedByUserId == user.Id &&
                workItem.CreatedAt >= from &&
                workItem.CreatedAt <= to),
            CompletedWorkItemsCount = completedItems.Count,
            InProgressWorkItemsCount = assignedItems.Count(workItem => IsInProgressStatus(workItem.Status)),
            BlockedWorkItemsCount = assignedItems.Count(workItem => workItem.IsBlocked && !IsDone(workItem.Status)),
            OverdueWorkItemsCount = assignedItems.Count(workItem => IsOverdue(workItem, now)),
            AssignedEffortPoints = assignedEffortPoints,
            CompletedEffortPoints = completedEffortPoints,
            CompletionPercentage = CalculatePercentage(completedItems.Count, assignedItems.Count),
            EffortCompletionPercentage = CalculatePercentage(completedEffortPoints, assignedEffortPoints),
            AverageCycleTimeDays = CalculateAverageCycleTimeDays(completedItems)
        };
    }

    private static TeamProductivityItemDto BuildTeamProductivityItem(
        TeamProjection team,
        IReadOnlyCollection<WorkItemMetricsProjection> workItems,
        DateTime from,
        DateTime to,
        DateTime now)
    {
        var teamWorkItems = workItems
            .Where(workItem => workItem.TeamId == team.Id)
            .ToList();

        var completedItems = teamWorkItems
            .Where(workItem => IsCompletedWithin(workItem, from, to))
            .ToList();

        var assignedEffortPoints = teamWorkItems.Sum(workItem => workItem.EffortPoints);
        var completedEffortPoints = completedItems.Sum(workItem => workItem.EffortPoints);

        return new TeamProductivityItemDto
        {
            TeamId = team.Id,
            TeamName = team.Name,
            TeamLeadId = team.TeamLeadId,
            TeamLeadName = team.TeamLeadName,
            Department = team.Department.ToString(),
            MembersCount = team.MembersCount,
            Capacity = team.Capacity,
            ActiveProjectsCount = team.ActiveProjectsCount,
            AssignedWorkItemsCount = teamWorkItems.Count,
            CompletedWorkItemsCount = completedItems.Count,
            InProgressWorkItemsCount = teamWorkItems.Count(workItem => IsInProgressStatus(workItem.Status)),
            BlockedWorkItemsCount = teamWorkItems.Count(workItem => workItem.IsBlocked && !IsDone(workItem.Status)),
            OverdueWorkItemsCount = teamWorkItems.Count(workItem => IsOverdue(workItem, now)),
            AssignedEffortPoints = assignedEffortPoints,
            CompletedEffortPoints = completedEffortPoints,
            CompletionPercentage = CalculatePercentage(completedItems.Count, teamWorkItems.Count),
            EffortCompletionPercentage = CalculatePercentage(completedEffortPoints, assignedEffortPoints),
            AverageCycleTimeDays = CalculateAverageCycleTimeDays(completedItems)
        };
    }

    private static ProjectHealthItemDto BuildProjectHealthItem(
        ProjectHealthProjection project,
        IReadOnlyCollection<WorkItemMetricsProjection> workItems,
        DateTime now)
    {
        var projectWorkItems = workItems
            .Where(workItem => workItem.ProjectId == project.Id)
            .ToList();

        var totalWorkItems = projectWorkItems.Count;
        var completedWorkItems = projectWorkItems.Count(workItem => IsDone(workItem.Status));
        var blockedWorkItems = projectWorkItems.Count(workItem => workItem.IsBlocked && !IsDone(workItem.Status));
        var overdueWorkItems = projectWorkItems.Count(workItem => IsOverdue(workItem, now));

        var totalBugs = projectWorkItems.Count(workItem => workItem.Type == WorkItemType.Bug);
        var criticalBugs = CountOpenBugsByPriority(projectWorkItems, WorkItemPriority.Critical);
        var highBugs = CountOpenBugsByPriority(projectWorkItems, WorkItemPriority.High);

        var totalEffortPoints = projectWorkItems.Sum(workItem => workItem.EffortPoints);
        var completedEffortPoints = projectWorkItems
            .Where(workItem => IsDone(workItem.Status))
            .Sum(workItem => workItem.EffortPoints);

        var completionPercentage = CalculatePercentage(completedWorkItems, totalWorkItems);
        var effortCompletionPercentage = CalculatePercentage(completedEffortPoints, totalEffortPoints);
        var blockedPercentage = CalculatePercentage(blockedWorkItems, totalWorkItems);
        var overduePercentage = CalculatePercentage(overdueWorkItems, totalWorkItems);

        var healthScore = CalculateProjectHealthScore(
            completionPercentage,
            blockedPercentage,
            overduePercentage,
            criticalBugs,
            highBugs);

        return new ProjectHealthItemDto
        {
            ProjectId = project.Id,
            ProjectKey = project.Key,
            ProjectName = project.Name,
            ProjectStatus = project.Status,
            TeamId = project.TeamId,
            TeamName = project.TeamName,
            TotalWorkItems = totalWorkItems,
            CompletedWorkItems = completedWorkItems,
            BlockedWorkItems = blockedWorkItems,
            OverdueWorkItems = overdueWorkItems,
            TotalBugs = totalBugs,
            CriticalBugs = criticalBugs,
            HighBugs = highBugs,
            TotalEffortPoints = totalEffortPoints,
            CompletedEffortPoints = completedEffortPoints,
            CompletionPercentage = completionPercentage,
            EffortCompletionPercentage = effortCompletionPercentage,
            BlockedPercentage = blockedPercentage,
            OverduePercentage = overduePercentage,
            HealthScore = healthScore,
            HealthStatus = GetHealthStatus(healthScore)
        };
    }

    private static BlockedWorkItemDto ToBlockedWorkItemDto(WorkItemDetailsProjection snapshot, DateTime now)
    {
        return new BlockedWorkItemDto
        {
            WorkItemId = snapshot.Id,
            Code = snapshot.Code,
            Title = snapshot.Title,
            Type = snapshot.Type,
            Status = snapshot.Status,
            Priority = snapshot.Priority,
            EffortPoints = snapshot.EffortPoints,
            BlockedAgeDays = CalculateNonNegativeDays(snapshot.CreatedAt, now),
            CreatedAt = snapshot.CreatedAt,
            EndDate = snapshot.EndDate,
            ProjectId = snapshot.ProjectId,
            ProjectKey = snapshot.ProjectKey,
            ProjectName = snapshot.ProjectName,
            AssignedToUserId = snapshot.AssignedToUserId,
            AssignedToUserName = snapshot.AssignedToUserName
        };
    }

    private static OverdueWorkItemDto ToOverdueWorkItemDto(WorkItemDetailsProjection snapshot, DateTime now)
    {
        return new OverdueWorkItemDto
        {
            WorkItemId = snapshot.Id,
            Code = snapshot.Code,
            Title = snapshot.Title,
            Type = snapshot.Type,
            Status = snapshot.Status,
            Priority = snapshot.Priority,
            EffortPoints = snapshot.EffortPoints,
            DaysOverdue = snapshot.EndDate.HasValue
                ? CalculateNonNegativeDays(snapshot.EndDate.Value, now)
                : 0,
            CreatedAt = snapshot.CreatedAt,
            EndDate = snapshot.EndDate!.Value,
            ProjectId = snapshot.ProjectId,
            ProjectKey = snapshot.ProjectKey,
            ProjectName = snapshot.ProjectName,
            AssignedToUserId = snapshot.AssignedToUserId,
            AssignedToUserName = snapshot.AssignedToUserName
        };
    }

    private static List<BugSeverityDistributionItemDto> BuildBugSeverityDistributionItems(
        IReadOnlyDictionary<WorkItemPriority, int> countsByPriority,
        int total)
    {
        return Enum.GetValues<WorkItemPriority>()
            .Select(priority =>
            {
                var count = countsByPriority.GetValueOrDefault(priority);

                return new BugSeverityDistributionItemDto
                {
                    Severity = priority,
                    Count = count,
                    Percentage = CalculatePercentage(count, total)
                };
            })
            .ToList();
    }

    private static List<WorkItemPriorityDistributionDto> BuildPriorityDistribution(
        IEnumerable<WorkItemPriority> priorities,
        int total)
    {
        var countsByPriority = CountBy(priorities);

        return Enum.GetValues<WorkItemPriority>()
            .Select(priority =>
            {
                var count = countsByPriority.GetValueOrDefault(priority);

                return new WorkItemPriorityDistributionDto
                {
                    Priority = priority,
                    Count = count,
                    Percentage = CalculatePercentage(count, total)
                };
            })
            .ToList();
    }

    private static List<WorkItemStatusDistributionDto> BuildStatusDistribution(
        IEnumerable<WorkItemStatus> statuses,
        int total)
    {
        var countsByStatus = CountBy(statuses);

        return Enum.GetValues<WorkItemStatus>()
            .Select(status =>
            {
                var count = countsByStatus.GetValueOrDefault(status);

                return new WorkItemStatusDistributionDto
                {
                    Status = status,
                    Count = count,
                    Percentage = CalculatePercentage(count, total)
                };
            })
            .ToList();
    }

    private static Dictionary<TEnum, int> CountBy<TEnum>(IEnumerable<TEnum> values)
        where TEnum : notnull
    {
        return values
            .GroupBy(value => value)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    private static bool HasDeveloperActivity(DeveloperProductivityItemDto developer)
    {
        return developer.AssignedWorkItemsCount > 0 ||
               developer.CreatedWorkItemsCount > 0 ||
               developer.CompletedWorkItemsCount > 0;
    }

    private static bool HasTeamActivity(TeamProductivityItemDto team)
    {
        return team.AssignedWorkItemsCount > 0 ||
               team.CompletedWorkItemsCount > 0 ||
               team.ActiveProjectsCount > 0;
    }

    private static bool IsCompletedWithin(WorkItemMetricsProjection workItem, DateTime from, DateTime to)
    {
        return IsDone(workItem.Status) &&
               workItem.EndDate.HasValue &&
               workItem.EndDate.Value >= from &&
               workItem.EndDate.Value <= to;
    }

    private static bool IsOverdue(WorkItemMetricsProjection workItem, DateTime now)
    {
        return !IsDone(workItem.Status) &&
               workItem.EndDate.HasValue &&
               workItem.EndDate.Value < now;
    }

    private static bool IsDone(WorkItemStatus status)
    {
        return status == WorkItemStatus.Done;
    }

    private static bool IsInProgressStatus(WorkItemStatus status)
    {
        return status is WorkItemStatus.InProgress
            or WorkItemStatus.InReview
            or WorkItemStatus.InTesting
            or WorkItemStatus.ReadyForRelease;
    }

    private static int CountOpenBugsByPriority(
        IEnumerable<WorkItemMetricsProjection> workItems,
        WorkItemPriority priority)
    {
        return workItems.Count(workItem =>
            workItem.Type == WorkItemType.Bug &&
            workItem.Priority == priority &&
            !IsDone(workItem.Status));
    }

    private static double CalculateMedianCycleTimeDays(List<WorkItemCycleTime> items)
    {
        if (items.Count == 0)
            return 0;

        var orderedValues = items
            .Select(item => item.CycleTimeDays)
            .OrderBy(value => value)
            .ToList();

        var middleIndex = orderedValues.Count / 2;

        return orderedValues.Count % 2 == 1
            ? orderedValues[middleIndex]
            : (orderedValues[middleIndex - 1] + orderedValues[middleIndex]) / 2.0;
    }

    private static double CalculateAverageCycleTimeDays(IEnumerable<WorkItemMetricsProjection> items)
    {
        var cycleTimes = items
            .Where(item => item.EndDate.HasValue)
            .Select(item => CalculateNonNegativeDays(item.StartDate ?? item.CreatedAt, item.EndDate!.Value))
            .ToList();

        return cycleTimes.Count > 0 ? cycleTimes.Average() : 0;
    }

    private static double CalculatePercentage(int value, int total)
    {
        return total > 0 ? (double) value / total * 100 : 0;
    }

    private static double AverageOrZero<T>(IReadOnlyCollection<T> items, Func<T, double> selector)
    {
        return items.Count > 0 ? items.Average(selector) : 0;
    }

    private static double AverageForItemsWithCompletedWork(IReadOnlyCollection<DeveloperProductivityItemDto> developers)
    {
        var activeDevelopers = developers
            .Where(developer => developer.CompletedWorkItemsCount > 0)
            .ToList();

        return AverageOrZero(activeDevelopers, developer => developer.AverageCycleTimeDays);
    }

    private static double AverageForTeamsWithCompletedWork(IReadOnlyCollection<TeamProductivityItemDto> teams)
    {
        var activeTeams = teams
            .Where(team => team.CompletedWorkItemsCount > 0)
            .ToList();

        return AverageOrZero(activeTeams, team => team.AverageCycleTimeDays);
    }

    private static int CalculateNonNegativeDays(DateTime from, DateTime to)
    {
        var days = (to - from).Days;
        return days < 0 ? 0 : days;
    }

    private static int NormalizeMaxItems(int maxItems)
    {
        return maxItems > 0 ? maxItems : DefaultMaxReportItems;
    }

    private static void ValidateDateRange(DateTime from, DateTime to)
    {
        if (from > to)
            throw new ArgumentException("The report start date cannot be after the report end date.");
    }

    private static double CalculateProjectHealthScore(
        double completionPercentage,
        double blockedPercentage,
        double overduePercentage,
        int criticalBugs,
        int highBugs)
    {
        var score = 100.0;

        score -= (100 - completionPercentage) * 0.20;
        score -= blockedPercentage * 0.30;
        score -= overduePercentage * 0.35;
        score -= Math.Min(30, criticalBugs * 10);
        score -= Math.Min(15, highBugs * 3);

        return ClampScore(score);
    }

    private static double ClampScore(double score)
    {
        return score < 0 ? 0 : score > 100 ? 100 : score;
    }

    private static string GetHealthStatus(double score)
    {
        return score >= 80 ? HealthStatusHealthy : score >= 60 ? HealthStatusAtRisk : HealthStatusCritical;
    }

    private sealed class ProjectLookup
    {
        public string Id { get; init; } = null!;
        public string Key { get; init; } = null!;
        public string Name { get; init; } = null!;
    }

    private sealed class UserProjection
    {
        public string Id { get; init; } = null!;
        public string FullName { get; init; } = null!;
        public string JobTitle { get; init; } = null!;
        public Department? Department { get; init; }
    }

    private sealed class TeamProjection
    {
        public string Id { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string TeamLeadId { get; init; } = null!;
        public string TeamLeadName { get; init; } = null!;
        public Department Department { get; init; }
        public int? Capacity { get; init; }
        public int MembersCount { get; init; }
        public int ActiveProjectsCount { get; init; }
    }

    private sealed class WorkItemCycleTimeProjection
    {
        public string Id { get; init; } = null!;
        public string Code { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }

    private sealed class SprintVelocityProjection
    {
        public string SprintId { get; init; } = null!;
        public string SprintCode { get; init; } = null!;
        public DateTime StartDate { get; init; }
        public int TotalStoryPoints { get; init; }
        public int CompletedStoryPoints { get; init; }
    }

    private sealed class WorkItemMetricsProjection
    {
        public string Id { get; init; } = null!;
        public string ProjectId { get; init; } = null!;
        public string? TeamId { get; init; }
        public string? AssignedToUserId { get; init; }
        public string CreatedByUserId { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public WorkItemType Type { get; init; }
        public WorkItemStatus Status { get; init; }
        public WorkItemPriority Priority { get; init; }
        public bool IsBlocked { get; init; }
        public int EffortPoints { get; init; }
    }

    private sealed class WorkItemDetailsProjection
    {
        public string Id { get; init; } = null!;
        public string Code { get; init; } = null!;
        public string Title { get; init; } = null!;
        public WorkItemType Type { get; init; }
        public WorkItemStatus Status { get; init; }
        public WorkItemPriority Priority { get; init; }
        public int EffortPoints { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? EndDate { get; init; }
        public string ProjectId { get; init; } = null!;
        public string ProjectKey { get; init; } = null!;
        public string ProjectName { get; init; } = null!;
        public string? AssignedToUserId { get; init; }
        public string? AssignedToUserName { get; init; }
    }

    private sealed class ProjectHealthProjection
    {
        public string Id { get; init; } = null!;
        public string Key { get; init; } = null!;
        public string Name { get; init; } = null!;
        public ProjectStatus Status { get; init; }
        public string? TeamId { get; init; }
        public string? TeamName { get; init; }
    }
}