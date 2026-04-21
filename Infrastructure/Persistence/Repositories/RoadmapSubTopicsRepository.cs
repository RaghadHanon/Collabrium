using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class RoadmapSubTopicsRepository : IRoadmapSubTopicsRepository
    {
        private readonly AppDbContext _dbContext;

        public RoadmapSubTopicsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(RoadmapSubTopic subTopic, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(subTopic);

            var entity = SubTopicsMapper.ToEntity(subTopic);

            await _dbContext.SubTopics.AddAsync(entity, ct);
        }

        public async Task<RoadmapSubTopic?> GetByIdAsync(
            string topicNodeId,
            string subTopicId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(topicNodeId) || string.IsNullOrWhiteSpace(subTopicId))
                return null;

            var tId = topicNodeId.Trim();
            var sId = subTopicId.Trim();

            var entity = await _dbContext.SubTopics
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Id == sId &&
                    s.TopicNodeId == tId, ct);

            return entity is null
                ? null
                : SubTopicsMapper.ToDomain(entity);
        }

        public async Task<List<RoadmapSubTopic>> GetByTopicIdAsync(
            string topicNodeId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(topicNodeId))
                return new();

            var tId = topicNodeId.Trim();

            var entities = await _dbContext.SubTopics
                .AsNoTracking()
                .Where(s => s.TopicNodeId == tId)
                .OrderBy(s => s.Order)
                .ToListAsync(ct);

            return entities.Select(SubTopicsMapper.ToDomain).ToList();
        }

        public async Task<int> GetMaxOrderAsync(
            string topicNodeId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(topicNodeId))
                return 0;

            var tId = topicNodeId.Trim();

            var max = await _dbContext.SubTopics
                .Where(s => s.TopicNodeId == tId)
                .Select(s => (int?)s.Order)
                .MaxAsync(ct);

            return max ?? 0;
        }

        public Task<bool> HasResourcesAsync(string subTopicId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(subTopicId))
                return Task.FromResult(false);

            var sId = subTopicId.Trim();

            return _dbContext.Resources
                .AsNoTracking()
                .AnyAsync(r => r.SubTopicId == sId, ct);
        }

        public async Task<bool> RemoveAsync(
            string topicNodeId,
            string subTopicId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(topicNodeId) || string.IsNullOrWhiteSpace(subTopicId))
                return false;

            var tId = topicNodeId.Trim();
            var sId = subTopicId.Trim();

            var entity = await _dbContext.SubTopics
                .FirstOrDefaultAsync(s =>
                    s.Id == sId &&
                    s.TopicNodeId == tId, ct);

            if (entity is null)
                return false;

            _dbContext.SubTopics.Remove(entity);
            return true;
        }

        public async Task UpdateAsync(RoadmapSubTopic subTopic, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(subTopic);

            var entity = await _dbContext.SubTopics
                .FirstOrDefaultAsync(s =>
                    s.Id == subTopic.Id &&
                    s.TopicNodeId == subTopic.TopicNodeId, ct);

            if (entity is null)
                throw new InvalidOperationException("SubTopic not found.");

            SubTopicsMapper.UpdateEntity(entity, subTopic);
        }
        public async Task SwapOrderAsync(
    string topicNodeId,
    string firstSubTopicId,
    string secondSubTopicId,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(topicNodeId) ||
                string.IsNullOrWhiteSpace(firstSubTopicId) ||
                string.IsNullOrWhiteSpace(secondSubTopicId))
            {
                throw new ArgumentException("TopicNodeId and subtopic ids are required.");
            }

            var topicId = topicNodeId.Trim();
            var firstId = firstSubTopicId.Trim();
            var secondId = secondSubTopicId.Trim();

            if (string.Equals(firstId, secondId, StringComparison.Ordinal))
                return;

            var first = await _dbContext.SubTopics
                .FirstOrDefaultAsync(s => s.Id == firstId && s.TopicNodeId == topicId, ct);

            var second = await _dbContext.SubTopics
                .FirstOrDefaultAsync(s => s.Id == secondId && s.TopicNodeId == topicId, ct);

            if (first is null || second is null)
                throw new InvalidOperationException("One or both subtopics were not found.");

            var firstOrder = first.Order;
            var secondOrder = second.Order;

            first.Order = 0;
            await _dbContext.SaveChangesAsync(ct);

            second.Order = firstOrder;
            await _dbContext.SaveChangesAsync(ct);

            first.Order = secondOrder;
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
