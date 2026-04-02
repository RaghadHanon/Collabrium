using API.Contracts.CourseAssignments;
using Application.Features.CourseAssignments.Commands;
using Application.Features.CourseAssignments.Queries;
using AutoMapper;

namespace API.MappingProfiles
{
    public class CourseAssignmentsApiMappingProfile : Profile
    {
        public CourseAssignmentsApiMappingProfile()
        {
            CreateMap<GetCourseAssignmentsRequest, GetCourseAssignments.Query>();
            CreateMap<AssignCourseToUserRequest, AssignCourseToUser.Command>();
        }
    }
}
