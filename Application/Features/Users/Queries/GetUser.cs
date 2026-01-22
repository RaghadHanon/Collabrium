using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Users.Queries;
public class GetUser
{
    public class Query : IRequest<Result<UserWithTenantDto>>
    {
        public string? UserId { get; set; }
    }

    public class Handler(
        ICurrentUserAccessor currentUserAccessor,
        IUsersRepository usersRepository,
        IMapper mapper) : IRequestHandler<Query, Result<UserWithTenantDto>>
    {
        public async Task<Result<UserWithTenantDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = request.UserId != null ? await usersRepository.GetByIdAsync(request.UserId) : await currentUserAccessor.GetUserAsync();
            if (user == null)
                return Result<UserWithTenantDto>.Failure("User not found", HttpStatusCode.NotFound);

            var userDto = mapper.Map<UserWithTenantDto>(user);

            return Result<UserWithTenantDto>.Success(userDto, HttpStatusCode.OK);
        }
    }
}
