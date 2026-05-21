using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Users.Queries;

public class GetUserSkills
{
    public class Query : IRequest<Result<List<TagDto>>>
    {
        public string? UserId { get; set; }
    }

    public class Handler(
        IUsersRepository usersRepository,
        IUsersReadModel usersReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor
    ) : IRequestHandler<Query, Result<List<TagDto>>>
    {
        public async Task<Result<List<TagDto>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser == null)
                return Result<List<TagDto>>.Failure(
                    "Current user is not found.",
                    HttpStatusCode.NotFound);

            var targetUserId = string.IsNullOrWhiteSpace(request.UserId)
                ? currentUser.Id
                : request.UserId;

            var user = await usersRepository.GetByIdAsync(targetUserId, cancellationToken);
            if (user is null)
                return Result<List<TagDto>>.Failure(
                    "User not found.",
                    HttpStatusCode.NotFound);

            if (!string.IsNullOrWhiteSpace(tenantContext.TenantId) &&
                !string.Equals(user.TenantId, tenantContext.TenantId, StringComparison.Ordinal))
            {
                return Result<List<TagDto>>.Failure(
                    "User not found in this tenant.",
                    HttpStatusCode.Forbidden);
            }

            var skills = await usersReadModel.GetUserSkillsAsync(
                targetUserId,
                cancellationToken);

            return Result<List<TagDto>>.Success(
                skills,
                HttpStatusCode.OK);
        }
    }
}