using Application.Common;
using Application.Features.AI.DTOs.Roadmaps;
using Application.Interfaces.AI;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.AI.Queries.Roadmaps;
public static class GenerateRoadmapFullDraft
{
    public sealed class Query : IRequest<Result<RoadmapFullDraftDto>>
    {
        public RoadmapPath Path { get; set; }
        public RoadmapLevel Level { get; set; }

        public string Goal { get; set; } = string.Empty;

        public int TopicsCount { get; set; } = 5;
        public int SubTopicsPerTopic { get; set; } = 3;
        public int ResourcesPerSubTopic { get; set; } = 2;
    }

    public sealed class Handler(IRoadmapAiDraftService draftService)
        : IRequestHandler<Query, Result<RoadmapFullDraftDto>>
    {
        public async Task<Result<RoadmapFullDraftDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var draft = await draftService.GenerateFullDraftAsync(
                request.Path,
                request.Level,
                request.Goal,
                request.TopicsCount,
                request.SubTopicsPerTopic,
                request.ResourcesPerSubTopic,
                cancellationToken);

            return Result<RoadmapFullDraftDto>.Success(
                draft,
                HttpStatusCode.OK);
        }
    }
}