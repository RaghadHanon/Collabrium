using Application.Features.CourseAssignments.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles
{
    public sealed class CourseAssignmentsReadProfile : Profile
    {
        public CourseAssignmentsReadProfile()
        {
            CreateMap<CourseAssignmentEntity, CourseAssignmentDto>();

            CreateMap<CourseAssignmentEntity, CourseAssignmentDetailsDto>()
                .ForMember(d => d.CourseTitle,
                    o => o.MapFrom(s => s.Course.Title))
                .ForMember(d => d.AssignedToUserName,
                    o => o.MapFrom(s => s.AssignedToUser.FullName))
                .ForMember(d => d.AssignedByUserName,
                    o => o.MapFrom(s => s.AssignedByUser.FullName));
        }
    }
}
