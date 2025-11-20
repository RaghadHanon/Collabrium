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
    public class Query : IRequest<Result<UserDto>>
    {
        public string? UserId { get; set; }
    }

    public class Handler(
        ICurrentUserAccessor currentUserAccessor,
        IUsersRepository usersRepository,
        IMapper mapper): IRequestHandler<Query, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            ApplicationUser? user = null;
            if(request.UserId != null)
                user = await usersRepository.GetByIdAsync(request.UserId);
            else
                user = await currentUserAccessor.GetUserAsync();

            if (user == null)
                return Result<UserDto>.Failure("User not found", HttpStatusCode.NotFound);

            var userDto = mapper.Map<UserDto>(user);

            return Result<UserDto>.Success(userDto, HttpStatusCode.OK);
        }
    }
}
