using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Features.Tags.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels
{
    public sealed class RoadmapsReadModel : IRoadmapsReadModel
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public RoadmapsReadModel(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<PagedList<RoadmapDto, DateTime?>> GetRoadmapsAsync(
            RoadmapsSearchAndFilterParametersDto parameters,
            CancellationToken cancellationToken = default)
        {
            parameters ??= new RoadmapsSearchAndFilterParametersDto();
            var pageSize = parameters.PageSize <= 10 ? 10 : parameters.PageSize;
            IQueryable<RoadmapEntity> query = _dbContext.Roadmaps.AsNoTracking();
            query = ApplyFilters(query, parameters);
            if (parameters.Cursor.HasValue)
                query = query.Where(r => r.CreatedAt < parameters.Cursor.Value);
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .ProjectTo<RoadmapDto>(_mapper.ConfigurationProvider)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);
            DateTime? nextCursor = null;
            if (items.Count > pageSize)
            {
                nextCursor = items[^1].CreatedAt;
                items.RemoveAt(items.Count - 1);
            }
            return new PagedList<RoadmapDto, DateTime?>
            {
                Items = items,
                NextCursor = nextCursor,
            };
        }
        private IQueryable<RoadmapEntity> ApplyFilters(IQueryable<RoadmapEntity> query,
            RoadmapsSearchAndFilterParametersDto parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.TenantId))
            {
                var tenantId = parameters.TenantId.Trim();
                query = query.Where(r => r.TenantId == tenantId);
            }
            if (!string.IsNullOrWhiteSpace(parameters.TitleContains))
            {
                var term = parameters.TitleContains.Trim();
                query = query.Where(r => r.Title != null && r.Title.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(parameters.DescriptionContains))
            {
                var term = parameters.DescriptionContains.Trim();
                query = query.Where(r => r.Description != null && r.Description.Contains(term));
            }
            if (parameters.Path.HasValue)
                query = query.Where(r => r.Path == parameters.Path.Value);

            if (parameters.Level.HasValue)
                query = query.Where(r => r.Level == parameters.Level.Value);

            if (parameters.IsVisible.HasValue)
                query = query.Where(r => r.IsVisible == parameters.IsVisible.Value);

            if (parameters.CreatedAfter.HasValue)
                query = query.Where(r => r.CreatedAt >= parameters.CreatedAfter.Value);

            if (parameters.CreatedBefore.HasValue)
                query = query.Where(r => r.CreatedAt <= parameters.CreatedBefore.Value);

            if (parameters.UpdatedAfter.HasValue)
                query = query.Where(r => r.UpdatedAt != null && r.UpdatedAt >= parameters.UpdatedAfter.Value);

            if (parameters.UpdatedBefore.HasValue)
                query = query.Where(r => r.UpdatedAt != null && r.UpdatedAt <= parameters.UpdatedBefore.Value);
            if (parameters.TagIdsAny is { Count: > 0 })
            {
                var tagsIds = parameters.TagIdsAny
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToList();
                if (tagsIds.Count > 0)
                {
                    query = query.Where(r =>
                        r.Tags.Any(rt => tagsIds.Contains(rt.TagId)));
                }
            }

            if (parameters.TagIdsAll is { Count: > 0 })
            {
                var tagIds = parameters.TagIdsAll
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToList();

                if (tagIds.Count > 0)
                {
                    query = query.Where(r =>
                        tagIds.All(tagId =>
                            r.Tags.Any(rt => rt.TagId == tagId)));
                }
            }

            return query;

        }
        public async Task<List<TagDto>> GetTagsAsync(
           string tenantId,
           string roadmapId,
           CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId))
                return new();

            var tId = tenantId.Trim();
            var rId = roadmapId.Trim();

            return await _dbContext.Roadmaps
                .AsNoTracking()
                .Where(r => r.TenantId == tId && r.Id == rId)
                .SelectMany(r => r.Tags)
                .Select(rt => rt.Tag)
                .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public Task<RoadmapDto?> GetByIdAsync(string tenantId, string roadmapId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId))
                return Task.FromResult<RoadmapDto?>(null);

            var tId = tenantId.Trim();
            var rId = roadmapId.Trim();
            return _dbContext.Roadmaps.
              AsNoTracking()
             .Where(r => r.TenantId == tId && r.Id == rId)
            .ProjectTo<RoadmapDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<RoadmapDetailsDto?> GetDetailsAsync(
            string tenantId,
            string roadmapId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId))
                return null;
            var tId = tenantId.Trim();
            var rId = roadmapId.Trim();
            return await _dbContext.Roadmaps
                .AsNoTracking()
                .Where(r => r.TenantId == tId && r.Id == rId)
                .Select(r => new RoadmapDetailsDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Path = r.Path,
                    Level = r.Level,
                    Topics = r.Nodes
                        .OfType<TopicNodeEntity>()
                        .OrderBy(n => n.Order)
                        .Select(t => new TopicDto
                        {
                            Id = t.Id,
                            Title = t.Title,
                            Description = t.Description,
                            Order = t.Order,

                            SubTopics = t.SubTopics
                                .OrderBy(s => s.Order)
                                .Select(s => new SubTopicDto
                                {
                                    Id = s.Id,
                                    Title = s.Title,
                                    Description = s.Description,
                                    Order = s.Order
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

    }
}
