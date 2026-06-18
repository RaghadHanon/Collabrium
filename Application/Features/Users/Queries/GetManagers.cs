using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Users.Queries;
public sealed class GetManagers
{
    public sealed class Query : IRequest<Result<List<UserDto>>>
    {
    }

    public sealed class Handler(
        IUsersReadModel usersReadModel,
        ITenantContext tenantContext)
        : IRequestHandler<Query, Result<List<UserDto>>>
    {
        public async Task<Result<List<UserDto>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<List<UserDto>>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            var result = await usersReadModel.GetManagersAsync(
                tenantId,
                cancellationToken);

            return Result<List<UserDto>>.Success(
                result,
                HttpStatusCode.OK);
        }
    }
}