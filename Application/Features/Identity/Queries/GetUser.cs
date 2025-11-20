using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Application.Features.Identity.Queries;
public class GetUser
{
    public class Query : IRequest<Result<UserWithTenantDto>>
    {
        public string? UserId { get; set; }
    }

    public class Handler(
        ICurrentUserAccessor currentUserAccessor,
        IUsersRepository usersRepository,
        IMapper mapper): IRequestHandler<Query, Result<UserWithTenantDto>>
    {
        public async Task<Result<UserWithTenantDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            ApplicationUser? user = null;
            if(request.UserId != null)
                user = await usersRepository.GetByIdAsync(request.UserId);
            else
                user = await currentUserAccessor.GetUserAsync();

            if (user == null)
                return Result<UserWithTenantDto>.Failure("User not found", HttpStatusCode.NotFound);

            var userDto = mapper.Map<UserWithTenantDto>(user);

            return Result<UserWithTenantDto>.Success(userDto, HttpStatusCode.OK);
        }
    }
}
