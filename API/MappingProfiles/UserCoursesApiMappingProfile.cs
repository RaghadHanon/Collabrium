using API.Contracts.UserCourses;
using Application.Features.UserCourses.Commands;
using Application.Features.UserCourses.Queries;
using AutoMapper;

namespace API.MappingProfiles
{
    public sealed class UserCoursesApiMappingProfile : Profile
    {
        public UserCoursesApiMappingProfile()
        {
            CreateMap<GetMyUserCoursesRequest, GetMyUserCourses.Query>();

            CreateMap<GetMySavedUserCoursesRequest, GetMySavedCourses.Query>();

            CreateMap<GetAssignedToMeCoursesRequest, GetAssignedToMeCourses.Query>();

            CreateMap<GetCoursesAssignedByMeRequest, GetCoursesAssignedByMe.Query>();

            CreateMap<AssignCourseToUserRequest, AssignCourseToUser.Command>();

            CreateMap<AssignCourseToTeamRequest, AssignCourseToTeam.Command>();

            CreateMap<UnassignCourseFromUserRequest, UnassignCourseFromUser.Command>();
        }
    }
}
