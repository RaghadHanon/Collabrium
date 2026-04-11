using Application.Features.CourseRatings.DTOs;
using Application.Features.CourseRatings.Queries;
using Application.Features.Courses.DTOs;
using Application.Features.Courses.Queries;
using Application.Features.ProjectHistories.DTOs;
using Application.Features.Projects.DTOs;
using Application.Features.Projects.Queries;
using Application.Features.ProjectsHistories.Queries;
using Application.Features.Roadmaps.DTOs;
using Application.Features.Roadmaps.Queries;
using Application.Features.Sprints.DTOs;
using Application.Features.Sprints.Queries;
using Application.Features.Tags.DTOs;
using Application.Features.Tags.Queries;
using Application.Features.Teams.DTOs;
using Application.Features.Teams.Queries;
using Application.Features.Tenants.DTOs;
using Application.Features.Tenants.Queries;
using Application.Features.UserCourses.DTOs;
using Application.Features.UserCourses.Queries;
using Application.Features.Users.DTOs;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.Comments;
using Application.Features.WorkItems.Queries;
using Application.Features.WorkItems.Queries.Comments;
using AutoMapper;

namespace Application.Common;
public class SearchAndFilterMappingProfile : Profile
{
    public SearchAndFilterMappingProfile()
    {
        CreateMap<GetTenantUsers.Query, UsersSearchAndFilterParametersDto>();
        CreateMap<GetTenants.Query, TenantsSearchAndFilterParametersDto>();
        CreateMap<GetTeams.Query, TeamsSearchAndFilterParametersDto>();
        CreateMap<GetUserTeams.Query, TeamsSearchAndFilterParametersDto>();
        CreateMap<GetSprints.Query, SprintsSearchAndFilterParametersDto>();
        CreateMap<GetProjects.Query, ProjectsSearchAndFilterParametersDto>();
        CreateMap<GetUserProjects.Query, ProjectsSearchAndFilterParametersDto>();
        CreateMap<GetWorkItems.Query, WorkItemsSearchAndFilterParametersDto>();
        CreateMap<GetProjectBoard.Query, WorkItemsSearchAndFilterParametersDto>();
        CreateMap<GetProjectBacklog.Query, WorkItemsSearchAndFilterParametersDto>();
        CreateMap<GetWorkItemComments.Query, CommentsQueryParametersDto>();
        CreateMap<GetTags.Query, TagsSearchAndFilterParametersDto>();
        CreateMap<GetRoadmaps.Query, RoadmapsSearchAndFilterParametersDto>();
        CreateMap<GetRoadmaps.Query, CoursesSearchAndFilterParametersDto>();
        CreateMap<GetProjectHistory.Query, ProjectHistorySearchAndFiltersParametersDto>();
        CreateMap<GetCourses.Query, CoursesSearchAndFilterParametersDto>();
        CreateMap<GetMyUserCourses.Query, UserCoursesSearchAndFilterParametersDto>();
        CreateMap<GetCourseRatings.Query, CourseRatingsSearchAndFilterParametersDto>();
        CreateMap<GetCoursesAssignedByMe.Query, AssignedUserCoursesSearchAndFilterParametersDto>();

    }
}
