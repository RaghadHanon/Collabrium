using Application.Features.Tenants.Queries;
using Application.Features.Users.DTOs;
using AutoMapper;
using Domain.Models;

namespace Application.Features.Users;
public class UsersMappingProfile : Profile
{
    public UsersMappingProfile()
    {
        CreateMap<User, UserWithTenantDto>();

        CreateMap<User, UserDto>()
            .ForMember(d => d.ProfileImageUrl, opt => opt.MapFrom(s => s.ProfileImage != null ? s.ProfileImage.Url : null));

        CreateMap<GetTenantUsers.Query, UsersSearchAndFilterParametersDto>();
    }
}