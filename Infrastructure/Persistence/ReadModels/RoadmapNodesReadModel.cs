using Application.Features.RoadmapNodes.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class RoadmapNodesReadModel : IRoadmapNodesReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public RoadmapNodesReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<RoadmapNodeDto>> GetByRoadmapIdAsync(
  string tenantId,
  string roadmapId,
  CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId))
            return new List<RoadmapNodeDto>();

        tenantId = tenantId.Trim();
        roadmapId = roadmapId.Trim();

        var topics = await _dbContext.RoadmapNodes
            .AsNoTracking()
            .OfType<TopicNodeEntity>()
            .Where(n => n.TenantId == tenantId && n.RoadmapId == roadmapId)
            .OrderBy(n => n.Order)
            .ProjectTo<TopicNodeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        var references = await _dbContext.RoadmapNodes
            .AsNoTracking()
            .OfType<ReferenceNodeEntity>()
            .Where(n => n.TenantId == tenantId && n.RoadmapId == roadmapId)
            .OrderBy(n => n.Order)
            .ProjectTo<ReferenceNodeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return topics
            .Cast<RoadmapNodeDto>()
            .Concat(references)
            .OrderBy(n => n.Order)
            .ToList();
    }

    public async Task<ReferenceNodeDetailsDto?> GetReferenceDetailsAsync(string tenantId, string roadmapId, string referenceNodeId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId) || string.IsNullOrWhiteSpace(referenceNodeId))
            return null;
        tenantId = tenantId.Trim();
        roadmapId = roadmapId.Trim();
        referenceNodeId = referenceNodeId.Trim();
        return await _dbContext.RoadmapNodes
           .AsNoTracking()
           .OfType<ReferenceNodeEntity>()
           .Where(n => n.TenantId == tenantId && n.RoadmapId == roadmapId && n.Id == referenceNodeId)
           .ProjectTo<ReferenceNodeDetailsDto>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync(ct);
    }
    public async Task<ReferenceNodeDto?> GetReferenceAsync(string tenantId, string roadmapId, string referenceNodeId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId) || string.IsNullOrWhiteSpace(referenceNodeId))
            return null;
        tenantId = tenantId.Trim();
        roadmapId = roadmapId.Trim();
        referenceNodeId = referenceNodeId.Trim();
        return await _dbContext.RoadmapNodes
           .AsNoTracking()
           .OfType<ReferenceNodeEntity>()
           .Where(n => n.TenantId == tenantId && n.RoadmapId == roadmapId && n.Id == referenceNodeId)
           .ProjectTo<ReferenceNodeDto>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync(ct);
    }
    public async Task<TopicNodeDto?> GetTopicAsync(string tenantId, string roadmapId, string topicNodeId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId) || string.IsNullOrWhiteSpace(topicNodeId))
            return null;
        tenantId = tenantId.Trim();
        roadmapId = roadmapId.Trim();
        topicNodeId = topicNodeId.Trim();
        return await _dbContext.RoadmapNodes
           .AsNoTracking()
           .OfType<TopicNodeEntity>()
           .Where(n => n.TenantId == tenantId && n.RoadmapId == roadmapId && n.Id == topicNodeId)
           .ProjectTo<TopicNodeDto>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync(ct);
    }

    public async Task<TopicNodeDetailsDto?> GetTopicDetailsAsync(string tenantId, string roadmapId, string topicNodeId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId) || string.IsNullOrWhiteSpace(topicNodeId))
            return null;
        tenantId = tenantId.Trim();
        roadmapId = roadmapId.Trim();
        topicNodeId = topicNodeId.Trim();
        return await _dbContext.RoadmapNodes
           .AsNoTracking()
           .OfType<TopicNodeEntity>()
           .Where(n => n.TenantId == tenantId && n.RoadmapId == roadmapId && n.Id == topicNodeId)
           .ProjectTo<TopicNodeDetailsDto>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync(ct);
    }

    public async Task<RoadmapNodeDto?> GetByIdAsync(
        string tenantId,
        string roadmapId,
        string nodeId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(roadmapId) ||
            string.IsNullOrWhiteSpace(nodeId))
            return null;

        tenantId = tenantId.Trim();
        roadmapId = roadmapId.Trim();
        nodeId = nodeId.Trim();

        var topic = await _dbContext.RoadmapNodes
            .AsNoTracking()
            .OfType<TopicNodeEntity>()
            .Where(n => n.TenantId == tenantId &&
                        n.RoadmapId == roadmapId &&
                        n.Id == nodeId)
            .ProjectTo<TopicNodeDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

        if (topic is not null)
            return topic;

        var reference = await _dbContext.RoadmapNodes
            .AsNoTracking()
            .OfType<ReferenceNodeEntity>()
            .Where(n => n.TenantId == tenantId &&
                        n.RoadmapId == roadmapId &&
                        n.Id == nodeId)
            .ProjectTo<ReferenceNodeDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

        return reference;
    }
}

