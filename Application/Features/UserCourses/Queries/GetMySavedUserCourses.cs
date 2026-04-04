using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.UserCourses.Queries
{
    public sealed class GetMySavedUserCourses
    {
        public sealed class Query : IRequest<Result<PagedList<UserCourseDto, DateTime?>>>
        {
            public string? Search { get; set; }

            public DateTime? CreatedAfter { get; set; }
            public DateTime? CreatedBefore { get; set; }

            public DateTime? UpdatedAfter { get; set; }
            public DateTime? UpdatedBefore { get; set; }

            public int PageSize { get; set; } = 10;
            public DateTime? Cursor { get; set; }
        }

        public sealed class Handler(
            IUserCoursesReadModel userCoursesReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor)
            : IRequestHandler<Query, Result<PagedList<UserCourseDto, DateTime?>>>
        {
            public async Task<Result<PagedList<UserCourseDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PagedList<UserCourseDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PagedList<UserCourseDto, DateTime?>>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var parameters = new UserCoursesSearchAndFilterParametersDto
                {
                    TenantId = tenantId,
                    UserId = currentUserId,
                    Search = request.Search,
                    IsSaved = true,

                    CreatedAfter = request.CreatedAfter,
                    CreatedBefore = request.CreatedBefore,

                    UpdatedAfter = request.UpdatedAfter,
                    UpdatedBefore = request.UpdatedBefore,

                    PageSize = request.PageSize,
                    Cursor = request.Cursor
                };

                var result = await userCoursesReadModel.GetUserCoursesAsync(parameters, cancellationToken);

                return Result<PagedList<UserCourseDto, DateTime?>>.Success(result, HttpStatusCode.OK);
            }
        }
    }
}
