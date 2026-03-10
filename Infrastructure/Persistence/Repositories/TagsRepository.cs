using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class TagsRepository : ITagsRepository
    {
        private readonly AppDbContext _dbContext;

        public TagsRepository(AppDbContext dbContext)
            => _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tag);
            var exists = await _dbContext.Tags
                .AnyAsync(t => t.TenantId == tag.TenantId && t.Name == tag.Name, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Tag with name '{tag.Name}' already exists for this tenant.");
            var entity = TagMapper.ToEntity(tag);
            await _dbContext.Tags.AddAsync(entity, cancellationToken);

        }

        public async Task<Tag?> GetByIdAsync(string tenantId, string tagId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(tagId))
                return null;

            var tId = tenantId.Trim();
            var id = tagId.Trim();

            var entity = await _dbContext.Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantId == tId && t.Id == id, cancellationToken);

            return entity is null ? null : TagMapper.ToDomain(entity);
        }

        public async Task<bool> IsNameUniqueAsync(
            string tenantId,
            string name,
            CancellationToken cancellationToken,
            string? excludeTagId = null)
        {
            var query = _dbContext.Tags
                .Where(t =>
                    t.TenantId == tenantId &&
                    t.Name.ToLower() == name.ToLower());

            if (!string.IsNullOrWhiteSpace(excludeTagId))
            {
                query = query.Where(t => t.Id != excludeTagId);
            }

            var exists = await query.AnyAsync(cancellationToken);

            return !exists;
        }

        public async Task<bool> RemoveAsync(string tenantId, string tagId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(tagId))
                return false;

            var tId = tenantId.Trim();
            var id = tagId.Trim();

            var entity = await _dbContext.Tags
                .FirstOrDefaultAsync(t => t.TenantId == tId && t.Id == id, cancellationToken);

            if (entity is null) return false;

            _dbContext.Tags.Remove(entity);
            return true;
        }

        public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tag);

            var entity = await _dbContext.Tags
                .FirstOrDefaultAsync(t => t.Id == tag.Id, cancellationToken);

            if (entity is null)
                throw new InvalidOperationException("Tag not found.");

            // Optional: Check for name uniqueness if changed
            if (!string.Equals(entity.Name, tag.Name, StringComparison.Ordinal))
            {
                var exists = await _dbContext.Tags
                    .AnyAsync(t => t.TenantId == tag.TenantId && t.Name == tag.Name && t.Id != tag.Id, cancellationToken);
                if (exists)
                    throw new InvalidOperationException($"Another tag with name '{tag.Name}' already exists for this tenant.");
            }
            TagMapper.UpdateEntity(tag, entity);
        }
    }
}
