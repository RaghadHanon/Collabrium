using Application.Features.RoadmapResources.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels
{
    public sealed class RoadmapResourcesReadModel : IRoadmapResourcesReadModel
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public RoadmapResourcesReadModel(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public Task<ResourceDto?> GetByIdAsync(
            string tenantId,
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            string resourceId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId)
                || string.IsNullOrWhiteSpace(roadmapId)
                || string.IsNullOrWhiteSpace(topicNodeId)
                || string.IsNullOrWhiteSpace(subTopicId)
                || string.IsNullOrWhiteSpace(resourceId))
                return Task.FromResult<ResourceDto?>(null);

            var tId = tenantId.Trim();
            var rId = roadmapId.Trim();
            var tNodeId = topicNodeId.Trim();
            var sId = subTopicId.Trim();
            var resId = resourceId.Trim();

            return _dbContext.Resources
                .AsNoTracking()
                .Where(x =>
                    x.Id == resId
                    && x.SubTopicId == sId
                    && x.SubTopic.TopicNodeId == tNodeId
                    && x.SubTopic.TopicNode.RoadmapId == rId
                    && x.SubTopic.TopicNode.TenantId == tId)
                .ProjectTo<ResourceDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<ResourceDto>> GetBySubTopicIdAsync(
            string tenantId,
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId)
                || string.IsNullOrWhiteSpace(roadmapId)
                || string.IsNullOrWhiteSpace(topicNodeId)
                || string.IsNullOrWhiteSpace(subTopicId))
                return new List<ResourceDto>();

            var tId = tenantId.Trim();
            var rId = roadmapId.Trim();
            var tNodeId = topicNodeId.Trim();
            var sId = subTopicId.Trim();

            return await _dbContext.Resources
                .AsNoTracking()
                .Where(x =>
                    x.SubTopicId == sId
                    && x.SubTopic.TopicNodeId == tNodeId
                    && x.SubTopic.TopicNode.RoadmapId == rId
                    && x.SubTopic.TopicNode.TenantId == tId)
                .OrderBy(x => x.Order)
                .ProjectTo<ResourceDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);
        }
    }
}
