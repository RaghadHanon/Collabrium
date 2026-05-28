using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
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
        IUsersReadModel usersReadModel)
        : IRequestHandler<Query, Result<UserWithTenantDto>>
    {
        public async Task<Result<UserWithTenantDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = request.UserId ?? currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Result<UserWithTenantDto>.Failure("User not found", HttpStatusCode.NotFound);

            var UserWithTenantDto = await usersReadModel.GetUserWithTenantAsync(userId, cancellationToken);
            return UserWithTenantDto == null
                ? Result<UserWithTenantDto>.Failure("User not found", HttpStatusCode.NotFound)
                : Result<UserWithTenantDto>.Success(UserWithTenantDto, HttpStatusCode.OK);
        }
    }
}