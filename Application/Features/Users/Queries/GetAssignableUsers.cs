using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Users.Queries
{
    public sealed class GetAssignableUsers
    {
        public sealed class Query : IRequest<Result<PagedList<AssignableUserDto, DateTime?>>>
        {
            public string? Search { get; set; }
            public int PageSize { get; set; } = 10;
            public DateTime? Cursor { get; set; }
        }

        public sealed class Handler(
            IUsersReadModel usersReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor)
            : IRequestHandler<Query, Result<PagedList<AssignableUserDto, DateTime?>>>
        {
            public async Task<Result<PagedList<AssignableUserDto, DateTime?>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PagedList<AssignableUserDto, DateTime?>>
                        .Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();

                var parameters = new AssignableUsersSearchAndFilterParametersDto
                {
                    TenantId = tenantId,
                    CurrentUserId = currentUserId,
                    Search = request.Search,
                    PageSize = request.PageSize,
                    Cursor = request.Cursor
                };

                var result = await usersReadModel.GetAssignableUsersAsync(parameters, cancellationToken);

                return Result<PagedList<AssignableUserDto, DateTime?>>
                    .Success(result, HttpStatusCode.OK);
            }
        }
    }
}
