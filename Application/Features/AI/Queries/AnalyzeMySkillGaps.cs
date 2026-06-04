using Application.Common;
using Application.Features.AI.DTOs.SkillGaps;
using Application.Interfaces;
using Application.Interfaces.AI;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.AI.Queries;
public static class AnalyzeMySkillGaps
{
    public sealed class Query : IRequest<Result<SkillGapAnalysisDto>>
    {
    }

    public sealed class Handler(
        IUserSkillGapAiService skillGapAiService,
        IUsersReadModel usersReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, Result<SkillGapAnalysisDto>>
    {
        public async Task<Result<SkillGapAnalysisDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id ?? tenantContext.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<SkillGapAnalysisDto>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);
            }

            var currentUser = await currentUserAccessor.GetCurrentUserAsync(cancellationToken);

            if (currentUser is null)
            {
                return Result<SkillGapAnalysisDto>.Failure(
                    "Current user not found.",
                    HttpStatusCode.Unauthorized);
            }

            var userRole = await currentUserAccessor.GetUserRole(
                currentUser.Id,
                cancellationToken);

            if (userRole is null)
            {
                return Result<SkillGapAnalysisDto>.Failure(
                    "Current user role not found.",
                    HttpStatusCode.NotFound);
            }

            var skills = await usersReadModel.GetUserSkillsAsync(
                currentUser.Id,
                cancellationToken);

            var skillNames = skills
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var role = userRole.Value.ToString();

            var analysis = await skillGapAiService.AnalyzeAsync(
                role,
                skillNames,
                cancellationToken);

            return Result<SkillGapAnalysisDto>.Success(
                analysis,
                HttpStatusCode.OK);
        }
    }
}