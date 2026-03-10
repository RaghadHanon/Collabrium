using Application.Common;
using Application.Features.Reports.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Reports.Queries;
public class GetTenantInfoReport
{
    public class Query : IRequest<Result<TenantInfoReport>>
    {
        public string? TenantId { get; set; }
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<TenantInfoReport>>
    {
        public async Task<Result<TenantInfoReport>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.TenantId is null)
            {
                request.TenantId = tenantContext.TenantId;
                if (request.TenantId is null)
                {
                    return Result<TenantInfoReport>.Failure("TenantId is not found.", HttpStatusCode.BadRequest);
                }
            }

            var report = await reportsReadModel.GenerateTenantInfoReportAsync(request.TenantId);
            return Result<TenantInfoReport>.Success(report, HttpStatusCode.OK);
        }
    }
}