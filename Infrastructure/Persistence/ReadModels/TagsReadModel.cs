using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels
{
    public sealed class TagsReadModel : ITagsReadModel
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public TagsReadModel(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<PagedList<TagDto, DateTime?>> GetAsync(TagsSearchParametersDto parameters,
            CancellationToken cancellationToken = default)
        {
            parameters ??= new TagsSearchParametersDto();
            var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;
            IQueryable<TagEntity> query = _dbContext.Tags.AsNoTracking();
            // Tenant filter
            if (!string.IsNullOrWhiteSpace(parameters.TenantId))
            {
                var tenantId = parameters.TenantId.Trim();
                query = query.Where(t => t.TenantId == tenantId);
            }

            // Name search
            if (!string.IsNullOrWhiteSpace(parameters.NameContains))
            {
                var term = parameters.NameContains.Trim();
                query = query.Where(t => t.Name.Contains(term));
            }
            var items = await query.OrderByDescending(t => t.CreatedAt)
                .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            DateTime? nextCursor = null;
            if (items.Count > pageSize)
            {
                nextCursor = items[^1].CreatedAt;
                items.RemoveAt(items.Count - 1);
            }

            return new PagedList<TagDto, DateTime?>
            {
                Items = items,
                NextCursor = nextCursor
            };
        }
        public Task<TagDto?> GetByIdAsync(string tenantId, string tagId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(tagId))
                return Task.FromResult<TagDto?>(null);

            var tId = tenantId.Trim();
            var id = tagId.Trim();
            return _dbContext.Tags
                .AsNoTracking()
                .Where(t => t.TenantId == tId && t.Id == id)
                .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

    }
}
