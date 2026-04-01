using API.Contracts.CourseAssignments;
using Application.Features.CourseAssignments.Queries;
using AutoMapper;

namespace API.MappingProfiles
{
    public class CourseAssignmentsApiMappingProfile : Profile
    {
        public CourseAssignmentsApiMappingProfile()
        {
            CreateMap<GetCourseAssignmentsRequest, GetCourseAssignments.Query>();
        }
    }
}
