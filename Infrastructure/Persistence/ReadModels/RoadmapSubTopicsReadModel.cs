using Application.Features.Roadmaps.DTOs;
using Application.Features.SubTopics.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels
{
    public sealed class RoadmapSubTopicsReadModel : IRoadmapSubTopicsReadModel
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public RoadmapSubTopicsReadModel(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public Task<SubTopicDto?> GetByIdAsync(
             string tenantId,
             string roadmapId,
             string topicNodeId,
             string subTopicId,
             CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId) ||
                string.IsNullOrWhiteSpace(topicNodeId) || string.IsNullOrWhiteSpace(subTopicId))
                return Task.FromResult<SubTopicDto?>(null);

            tenantId = tenantId.Trim();
            roadmapId = roadmapId.Trim();
            topicNodeId = topicNodeId.Trim();
            subTopicId = subTopicId.Trim();

            return _dbContext.SubTopics
                .AsNoTracking()
                .Where(s =>
                    s.Id == subTopicId &&
                    s.TopicNodeId == topicNodeId &&
                    s.TopicNode.RoadmapId == roadmapId &&
                    s.TopicNode.Roadmap.TenantId == tenantId)
                .ProjectTo<SubTopicDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<SubTopicDto>> GetByTopicAsync(string tenantId, string roadmapId, string topicNodeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) ||
                string.IsNullOrWhiteSpace(roadmapId) ||
                string.IsNullOrWhiteSpace(topicNodeId))
                return new();
            tenantId = tenantId.Trim();
            roadmapId = roadmapId.Trim();
            topicNodeId = topicNodeId.Trim();

            return await _dbContext.SubTopics
                 .AsNoTracking()
                 .Where(st => st.TopicNode.RoadmapId == roadmapId &&
                              st.TopicNode.TenantId == tenantId &&
                              st.TopicNodeId == topicNodeId)
                 .OrderBy(s => s.Order)
                 .ProjectTo<SubTopicDto>(_mapper.ConfigurationProvider)
                 .ToListAsync(cancellationToken);

        }

        public async Task<SubTopicDetailsDto?> GetDetailsAsync(string tenantId, string roadmapId, string topicNodeId, string subTopicId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId) ||
                string.IsNullOrWhiteSpace(topicNodeId) || string.IsNullOrWhiteSpace(subTopicId))
                return null;

            var tId = tenantId.Trim();
            var rId = roadmapId.Trim();
            var topicId = topicNodeId.Trim();
            var sId = subTopicId.Trim();

            return await _dbContext.SubTopics
                .AsNoTracking()
                .Where(s =>
                    s.Id == sId &&
                    s.TopicNodeId == topicId &&
                    s.TopicNode.RoadmapId == rId &&
                    s.TopicNode.TenantId == tId)
                .Select(s => new SubTopicDetailsDto
                {
                    Id = s.Id,
                    TopicNodeId = s.TopicNodeId,
                    Title = s.Title,
                    Description = s.Description,
                    Order = s.Order,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    ResourcesCount = s.Resources.Count,
                    Resources = s.Resources
                        .OrderBy(r => r.Order)
                        .Select(r => new ResourceDto
                        {
                            Id = r.Id,
                            SubTopicId = r.SubTopicId,
                            Title = r.Title,
                            Url = r.Url,
                            Type = r.Type,
                            Order = r.Order
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);
        }

        public Task<bool> TopicExistsInRoadmapAsync(string tenantId, string roadmapId, string topicNodeId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId) || string.IsNullOrWhiteSpace(topicNodeId))
                return Task.FromResult(false);

            var tId = tenantId.Trim();
            var rId = roadmapId.Trim();
            var topicId = topicNodeId.Trim();

            return _dbContext.RoadmapNodes
                .AsNoTracking()
                .OfType<TopicNodeEntity>()
                .AnyAsync(t =>
                    t.Id == topicId &&
                    t.RoadmapId == rId &&
                    t.Roadmap.TenantId == tId, ct);
        }
    }
}

