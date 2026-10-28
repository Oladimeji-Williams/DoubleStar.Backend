using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;

using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Domain.Entities;
using DoubleStar.Modules.Reviews.Domain.Enums;

namespace DoubleStar.Modules.Reviews.Persistence.Repositories;

internal sealed class ReviewRepository(
    ReviewsDbContext dbContext)
    : IReviewRepository
{
    public Task<Review?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Reviews
            .FirstOrDefaultAsync(
                r => r.Id == id,
                cancellationToken);

    public async Task<IReadOnlyList<Review>> GetApprovedAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Reviews
            .Where(r => r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<Review>> GetPendingPagedAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = query.NormalizedPage;
        var normalizedPageSize = query.NormalizedPageSize;

        var reviews =
            dbContext.Reviews
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(r =>
                    r.Status == ReviewStatus.Pending &&
                    !r.IsDeleted);

        reviews = ApplySearch(
            reviews,
            query.Search);

        reviews = ApplyFilters(
            reviews,
            query.Filters);

        var totalCount =
            await reviews.CountAsync(
                cancellationToken);

        reviews = ApplySorting(
            reviews,
            query.SortBy,
            query.SortDescending);

        var items =
            await reviews
                .Skip(
                    (normalizedPage - 1) *
                    normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<Review>(
            items,
            normalizedPage,
            normalizedPageSize,
            totalCount);
    }

    public async Task<PagedResult<Review>> GetAllPagedAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = query.NormalizedPage;
        var normalizedPageSize = query.NormalizedPageSize;

        var reviews =
            dbContext.Reviews
                .IgnoreQueryFilters()
                .AsNoTracking();

        reviews = ApplySearch(
            reviews,
            query.Search);

        reviews = ApplyFilters(
            reviews,
            query.Filters);

        var totalCount =
            await reviews.CountAsync(
                cancellationToken);

        reviews = ApplySorting(
            reviews,
            query.SortBy,
            query.SortDescending);

        var items =
            await reviews
                .Skip(
                    (normalizedPage - 1) *
                    normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<Review>(
            items,
            normalizedPage,
            normalizedPageSize,
            totalCount);
    }

    public async Task AddAsync(
        Review review,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Reviews.AddAsync(
            review,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UpdateAsync(
        Review review,
        CancellationToken cancellationToken = default)
    {
        dbContext.Reviews.Update(review);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static IQueryable<Review> ApplySearch(
        IQueryable<Review> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();

        return query.Where(r =>
            r.ReviewerName.Contains(term) ||
            r.Comment.Contains(term));
    }

    private static IQueryable<Review> ApplyFilters(
        IQueryable<Review> query,
        IReadOnlyList<FilterCriterion>? filters)
    {
        if (filters is null || filters.Count == 0)
        {
            return query;
        }

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Field))
            {
                continue;
            }

            var field =
                filter.Field
                    .Trim()
                    .ToLowerInvariant();

            query = field switch
            {
                "id" =>
                    ApplyGuidFilter(
                        query,
                        r => r.Id,
                        filter),

                "reviewername" =>
                    ApplyTextFilter(
                        query,
                        r => r.ReviewerName,
                        filter),

                "rating" =>
                    ApplyRatingFilter(
                        query,
                        filter),

                "comment" =>
                    ApplyTextFilter(
                        query,
                        r => r.Comment,
                        filter),

                "status" =>
                    ApplyStatusFilter(
                        query,
                        filter),

                "isanonymous" =>
                    ApplyBooleanFilter(
                        query,
                        r => r.IsAnonymous,
                        filter),

                "isdeleted" =>
                    ApplyBooleanFilter(
                        query,
                        r => r.IsDeleted,
                        filter),

                _ => query
            };
        }

        return query;
    }

    private static IQueryable<Review> ApplyTextFilter(
        IQueryable<Review> query,
        Expression<Func<Review, string>> selector,
        FilterCriterion filter)
    {
        var value =
            filter.Value ?? string.Empty;

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    BuildEquals(
                        selector,
                        value)),

            FilterOperator.NotEquals =>
                query.Where(
                    BuildNotEquals(
                        selector,
                        value)),

            FilterOperator.Contains =>
                query.Where(
                    BuildContains(
                        selector,
                        value)),

            FilterOperator.StartsWith =>
                query.Where(
                    BuildStartsWith(
                        selector,
                        value)),

            FilterOperator.EndsWith =>
                query.Where(
                    BuildEndsWith(
                        selector,
                        value)),

            FilterOperator.IsEmpty =>
                query.Where(
                    BuildEquals(
                        selector,
                        string.Empty)),

            FilterOperator.IsNotEmpty =>
                query.Where(
                    BuildNotEquals(
                        selector,
                        string.Empty)),

            _ => query
        };
    }

    private static IQueryable<Review> ApplyGuidFilter(
        IQueryable<Review> query,
        Expression<Func<Review, Guid>> selector,
        FilterCriterion filter)
    {
        if (!Guid.TryParse(
                filter.Value,
                out var value))
        {
            return filter.Operator == FilterOperator.NotEquals
                ? query
                : query.Where(_ => false);
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    BuildEquals(
                        selector,
                        value)),

            FilterOperator.NotEquals =>
                query.Where(
                    BuildNotEquals(
                        selector,
                        value)),

            _ => query
        };
    }

    private static IQueryable<Review> ApplyRatingFilter(
        IQueryable<Review> query,
        FilterCriterion filter)
    {
        if (!int.TryParse(
                filter.Value,
                out var value))
        {
            return query.Where(_ => false);
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    r => r.Rating == value),

            FilterOperator.NotEquals =>
                query.Where(
                    r => r.Rating != value),

            FilterOperator.GreaterThan =>
                query.Where(
                    r => r.Rating > value),

            FilterOperator.GreaterThanOrEqual =>
                query.Where(
                    r => r.Rating >= value),

            FilterOperator.LessThan =>
                query.Where(
                    r => r.Rating < value),

            FilterOperator.LessThanOrEqual =>
                query.Where(
                    r => r.Rating <= value),

            FilterOperator.Between =>
                ApplyRatingBetween(
                    query,
                    filter),

            _ => query
        };
    }

    private static IQueryable<Review> ApplyRatingBetween(
        IQueryable<Review> query,
        FilterCriterion filter)
    {
        if (filter.Values is null ||
            filter.Values.Count < 2 ||
            !int.TryParse(
                filter.Values[0],
                out var minimum) ||
            !int.TryParse(
                filter.Values[1],
                out var maximum))
        {
            return query.Where(_ => false);
        }

        return query.Where(
            r =>
                r.Rating >= minimum &&
                r.Rating <= maximum);
    }

    private static IQueryable<Review> ApplyStatusFilter(
        IQueryable<Review> query,
        FilterCriterion filter)
    {
        if (!Enum.TryParse<ReviewStatus>(
                filter.Value,
                true,
                out var status))
        {
            return filter.Operator == FilterOperator.NotEquals
                ? query
                : query.Where(_ => false);
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    r => r.Status == status),

            FilterOperator.NotEquals =>
                query.Where(
                    r => r.Status != status),

            _ => query
        };
    }

    private static IQueryable<Review> ApplyBooleanFilter(
        IQueryable<Review> query,
        Expression<Func<Review, bool>> selector,
        FilterCriterion filter)
    {
        if (!bool.TryParse(
                filter.Value,
                out var value))
        {
            return query.Where(_ => false);
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    BuildEquals(
                        selector,
                        value)),

            FilterOperator.NotEquals =>
                query.Where(
                    BuildNotEquals(
                        selector,
                        value)),

            _ => query
        };
    }

    private static IQueryable<Review> ApplySorting(
        IQueryable<Review> query,
        string? sortBy,
        bool sortDescending)
    {
        return (sortBy ?? string.Empty)
            .Trim()
            .ToLowerInvariant() switch
        {
            "id" => sortDescending
                ? query.OrderByDescending(
                    r => r.Id)
                : query.OrderBy(
                    r => r.Id),

            "reviewername" => sortDescending
                ? query.OrderByDescending(
                    r => r.ReviewerName)
                : query.OrderBy(
                    r => r.ReviewerName),

            "rating" => sortDescending
                ? query.OrderByDescending(
                    r => r.Rating)
                : query.OrderBy(
                    r => r.Rating),

            "status" => sortDescending
                ? query.OrderByDescending(
                    r => r.Status)
                : query.OrderBy(
                    r => r.Status),

            "isanonymous" => sortDescending
                ? query.OrderByDescending(
                    r => r.IsAnonymous)
                : query.OrderBy(
                    r => r.IsAnonymous),

            "isdeleted" => sortDescending
                ? query.OrderByDescending(
                    r => r.IsDeleted)
                : query.OrderBy(
                    r => r.IsDeleted),

            "createdat" or "createdatutc" => sortDescending
                ? query.OrderByDescending(
                    r => r.CreatedAt)
                : query.OrderBy(
                    r => r.CreatedAt),

            _ =>
                query.OrderByDescending(
                    r => r.CreatedAt)
        };
    }

    private static Expression<Func<Review, bool>> BuildEquals(
        Expression<Func<Review, string>> selector,
        string value)
    {
        var parameter =
            selector.Parameters[0];

        return Expression.Lambda<Func<Review, bool>>(
            Expression.Equal(
                selector.Body,
                Expression.Constant(value)),
            parameter);
    }

    private static Expression<Func<Review, bool>> BuildNotEquals(
        Expression<Func<Review, string>> selector,
        string value)
    {
        var parameter =
            selector.Parameters[0];

        return Expression.Lambda<Func<Review, bool>>(
            Expression.NotEqual(
                selector.Body,
                Expression.Constant(value)),
            parameter);
    }

    private static Expression<Func<Review, bool>> BuildContains(
        Expression<Func<Review, string>> selector,
        string value)
    {
        var parameter =
            selector.Parameters[0];

        var method =
            typeof(string).GetMethod(
                nameof(string.Contains),
                [typeof(string)])!;

        return Expression.Lambda<Func<Review, bool>>(
            Expression.Call(
                selector.Body,
                method,
                Expression.Constant(value)),
            parameter);
    }

    private static Expression<Func<Review, bool>> BuildStartsWith(
        Expression<Func<Review, string>> selector,
        string value)
    {
        var parameter =
            selector.Parameters[0];

        var method =
            typeof(string).GetMethod(
                nameof(string.StartsWith),
                [typeof(string)])!;

        return Expression.Lambda<Func<Review, bool>>(
            Expression.Call(
                selector.Body,
                method,
                Expression.Constant(value)),
            parameter);
    }

    private static Expression<Func<Review, bool>> BuildEndsWith(
        Expression<Func<Review, string>> selector,
        string value)
    {
        var parameter =
            selector.Parameters[0];

        var method =
            typeof(string).GetMethod(
                nameof(string.EndsWith),
                [typeof(string)])!;

        return Expression.Lambda<Func<Review, bool>>(
            Expression.Call(
                selector.Body,
                method,
                Expression.Constant(value)),
            parameter);
    }

    private static Expression<Func<Review, bool>> BuildEquals(
        Expression<Func<Review, Guid>> selector,
        Guid value)
    {
        var parameter =
            selector.Parameters[0];

        return Expression.Lambda<Func<Review, bool>>(
            Expression.Equal(
                selector.Body,
                Expression.Constant(value)),
            parameter);
    }

    private static Expression<Func<Review, bool>> BuildNotEquals(
        Expression<Func<Review, Guid>> selector,
        Guid value)
    {
        var parameter =
            selector.Parameters[0];

        return Expression.Lambda<Func<Review, bool>>(
            Expression.NotEqual(
                selector.Body,
                Expression.Constant(value)),
            parameter);
    }

    private static Expression<Func<Review, bool>> BuildEquals(
        Expression<Func<Review, bool>> selector,
        bool value)
    {
        var parameter =
            selector.Parameters[0];

        return Expression.Lambda<Func<Review, bool>>(
            Expression.Equal(
                selector.Body,
                Expression.Constant(value)),
            parameter);
    }

    private static Expression<Func<Review, bool>> BuildNotEquals(
        Expression<Func<Review, bool>> selector,
        bool value)
    {
        var parameter =
            selector.Parameters[0];

        return Expression.Lambda<Func<Review, bool>>(
            Expression.NotEqual(
                selector.Body,
                Expression.Constant(value)),
            parameter);
    }
}