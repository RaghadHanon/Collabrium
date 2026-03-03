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
                .FirstOrDefaultAsync(s => s.Id == subTopic.Id && s.TopicNodeId == subTopic.TopicNodeId, ct);

            if (entity is null)
                throw new InvalidOperationException("SubTopic not found.");

            SubTopicsMapper.UpdateEntity(entity, subTopic);
        }
    }
}
