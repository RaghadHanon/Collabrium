using Application.Common;
using Application.Features.CourseRatings.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class CourseRatingsReadModel : ICourseRatingsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public CourseRatingsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public Task<CourseRatingDto?> GetByIdAsync(
        string tenantId,
        string ratingId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(ratingId))
            return Task.FromResult<CourseRatingDto?>(null);

        var tId = tenantId.Trim();
        var rId = ratingId.Trim();

        return _dbContext.CourseRatings
            .AsNoTracking()
            .Where(x => x.TenantId == tId && x.Id == rId)
            .ProjectTo<CourseRatingDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public Task<CourseRatingDto?> GetByUserAndCourseAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(courseId))
            return Task.FromResult<CourseRatingDto?>(null);

        var tId = tenantId.Trim();
        var uId = userId.Trim();
        var cId = courseId.Trim();

        return _dbContext.CourseRatings
            .AsNoTracking()
            .Where(x => x.TenantId == tId &&
                        x.UserId == uId &&
                        x.CourseId == cId)
            .ProjectTo<CourseRatingDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public Task<CourseRatingDetailsDto?> GetDetailsByIdAsync(
        string tenantId,
        string ratingId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(ratingId))
            return Task.FromResult<CourseRatingDetailsDto?>(null);

        var tId = tenantId.Trim();
        var rId = ratingId.Trim();

        return _dbContext.CourseRatings
            .AsNoTracking()
            .Where(x => x.TenantId == tId && x.Id == rId)
            .ProjectTo<CourseRatingDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedList<CourseRatingDto, DateTime?>> GetCourseRatingsAsync(
        CourseRatingsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default)
    {
        parameters ??= new CourseRatingsSearchAndFilterParametersDto();

        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.CourseRatings.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt < parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<CourseRatingDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        DateTime? nextCursor = null;

        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<CourseRatingDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<CourseRatingEntity> ApplyFilters(
        IQueryable<CourseRatingEntity> query,
        CourseRatingsSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TenantId))
        {
            var tId = p.TenantId.Trim();
            query = query.Where(x => x.TenantId == tId);
        }

        if (!string.IsNullOrWhiteSpace(p.CourseId))
        {
            var cId = p.CourseId.Trim();
            query = query.Where(x => x.CourseId == cId);
        }

        if (!string.IsNullOrWhiteSpace(p.UserId))
        {
            var uId = p.UserId.Trim();
            query = query.Where(x => x.UserId == uId);
        }

        if (p.MinRating.HasValue)
            query = query.Where(x => x.Rating >= p.MinRating.Value);

        if (p.MaxRating.HasValue)
            query = query.Where(x => x.Rating <= p.MaxRating.Value);

        if (p.CreatedAfter.HasValue)
            query = query.Where(x => x.CreatedAt >= p.CreatedAfter.Value);

        if (p.CreatedBefore.HasValue)
            query = query.Where(x => x.CreatedAt <= p.CreatedBefore.Value);

        if (p.UpdatedAfter.HasValue)
            query = query.Where(x => x.UpdatedAt != null && x.UpdatedAt >= p.UpdatedAfter.Value);

        if (p.UpdatedBefore.HasValue)
            query = query.Where(x => x.UpdatedAt != null && x.UpdatedAt <= p.UpdatedBefore.Value);

        return query;
    }
}
