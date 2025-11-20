using Application.Features.Identity.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Identity;
public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>();
    }
}
