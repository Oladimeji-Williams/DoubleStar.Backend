using Microsoft.EntityFrameworkCore;

using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Domain.Entities;
using DoubleStar.Modules.Sales.Domain.Enums;

namespace DoubleStar.Modules.Sales.Persistence.Repositories;

public sealed class SaleRepository(
    SalesDbContext dbContext) : ISaleRepository
{
    public Task<Sale?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        dbContext.Sales
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(
                s => s.Id == id,
                cancellationToken);

    public async Task<PagedResult<Sale>> SearchAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default)
    {
        var page = request.NormalizedPage;
        var pageSize = request.NormalizedPageSize;

        IQueryable<Sale> query =
            dbContext.Sales
                .AsNoTracking();

        query = ApplySearch(
            query,
            request.Search);

        query = ApplyFilters(
            query,
            request.Filters);

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        query = ApplySorting(
            query,
            request.SortBy,
            request.SortDescending);

        var items =
            await query
                .Include(s => s.Lines)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<Sale>(
            items,
            page,
            pageSize,
            totalCount);
    }

    public async Task<PagedResult<Sale>> SearchForCustomerAsync(
        Guid customerId,
        PagedQuery request,
        CancellationToken cancellationToken = default)
    {
        var page = request.NormalizedPage;
        var pageSize = request.NormalizedPageSize;

        IQueryable<Sale> query =
            dbContext.Sales
                .AsNoTracking()
                .Where(s =>
                    s.CustomerId == customerId &&
                    s.Status == SaleStatus.Completed);

        query = ApplySearch(
            query,
            request.Search);

        query = ApplyFilters(
            query,
            request.Filters);

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        query = ApplySorting(
            query,
            request.SortBy,
            request.SortDescending);

        var items =
            await query
                .Include(s => s.Lines)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<Sale>(
            items,
            page,
            pageSize,
            totalCount);
    }

    public async Task<IReadOnlyList<Sale>> GetAllForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Sales
            .Include(s => s.Lines)
            .Where(s =>
                s.CustomerId == customerId &&
                s.Status == SaleStatus.Completed)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Sale>> GetAllInRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default) =>
        await dbContext.Sales
            .Include(s => s.Lines)
            .Where(s =>
                s.Status == SaleStatus.Completed &&
                s.CompletedAtUtc >= fromUtc &&
                s.CompletedAtUtc <= toUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        Sale sale,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Sales.AddAsync(
            sale,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UpdateAsync(
        Sale sale,
        CancellationToken cancellationToken = default)
    {
        dbContext.Sales.Update(sale);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static IQueryable<Sale> ApplySearch(
        IQueryable<Sale> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        search = search.Trim();

        if (int.TryParse(search, out var id))
        {
            return query.Where(s =>
                s.Id == id);
        }

        return query;
    }

    private static IQueryable<Sale> ApplyFilters(
        IQueryable<Sale> query,
        IReadOnlyList<FilterCriterion>? filters)
    {
        if (filters is null ||
            filters.Count == 0)
        {
            return query;
        }

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Field))
            {
                continue;
            }

            switch (filter.Field.Trim().ToLowerInvariant())
            {
                case "id":
                    query = ApplyIdFilter(
                        query,
                        filter);
                    break;

                case "status":
                    query = ApplyStatusFilter(
                        query,
                        filter);
                    break;

                case "totalkobo":
                    query = ApplyTotalFilter(
                        query,
                        filter);
                    break;

                case "createdat":
                    query = ApplyCreatedAtFilter(
                        query,
                        filter);
                    break;
            }
        }

        return query;
    }

    private static IQueryable<Sale> ApplyIdFilter(
        IQueryable<Sale> query,
        FilterCriterion filter)
    {
        if (!int.TryParse(
                filter.Value,
                out var id))
        {
            return filter.Operator switch
            {
                FilterOperator.IsEmpty =>
                    query.Where(s => false),

                FilterOperator.IsNotEmpty =>
                    query.Where(s => true),

                _ =>
                    query.Where(s => false)
            };
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(s => s.Id == id),

            FilterOperator.NotEquals =>
                query.Where(s => s.Id != id),

            FilterOperator.GreaterThan =>
                query.Where(s => s.Id > id),

            FilterOperator.GreaterThanOrEqual =>
                query.Where(s => s.Id >= id),

            FilterOperator.LessThan =>
                query.Where(s => s.Id < id),

            FilterOperator.LessThanOrEqual =>
                query.Where(s => s.Id <= id),

            _ => query
        };
    }

    private static IQueryable<Sale> ApplyStatusFilter(
        IQueryable<Sale> query,
        FilterCriterion filter)
    {
        if (string.IsNullOrWhiteSpace(filter.Value))
        {
            return query;
        }

        if (!Enum.TryParse<SaleStatus>(
                filter.Value,
                true,
                out var status))
        {
            return filter.Operator switch
            {
                FilterOperator.NotEquals =>
                    query.Where(s => true),

                _ =>
                    query.Where(s => false)
            };
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(s => s.Status == status),

            FilterOperator.NotEquals =>
                query.Where(s => s.Status != status),

            _ => query
        };
    }

    private static IQueryable<Sale> ApplyTotalFilter(
        IQueryable<Sale> query,
        FilterCriterion filter)
    {
        if (!long.TryParse(
                filter.Value,
                out var total))
        {
            return query;
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(s => s.TotalKobo == total),

            FilterOperator.NotEquals =>
                query.Where(s => s.TotalKobo != total),

            FilterOperator.GreaterThan =>
                query.Where(s => s.TotalKobo > total),

            FilterOperator.GreaterThanOrEqual =>
                query.Where(s => s.TotalKobo >= total),

            FilterOperator.LessThan =>
                query.Where(s => s.TotalKobo < total),

            FilterOperator.LessThanOrEqual =>
                query.Where(s => s.TotalKobo <= total),

            _ => query
        };
    }

    private static IQueryable<Sale> ApplyCreatedAtFilter(
        IQueryable<Sale> query,
        FilterCriterion filter)
    {
        if (!DateTime.TryParse(
                filter.Value,
                out var date))
        {
            return query;
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(s =>
                    s.CreatedAt.Date == date.Date),

            FilterOperator.NotEquals =>
                query.Where(s =>
                    s.CreatedAt.Date != date.Date),

            FilterOperator.GreaterThan =>
                query.Where(s =>
                    s.CreatedAt > date),

            FilterOperator.GreaterThanOrEqual =>
                query.Where(s =>
                    s.CreatedAt >= date),

            FilterOperator.LessThan =>
                query.Where(s =>
                    s.CreatedAt < date),

            FilterOperator.LessThanOrEqual =>
                query.Where(s =>
                    s.CreatedAt <= date),

            _ => query
        };
    }

    private static IQueryable<Sale> ApplySorting(
        IQueryable<Sale> query,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "id" =>
                sortDescending
                    ? query
                        .OrderByDescending(s => s.Id)
                        .ThenByDescending(s => s.CreatedAt)
                    : query
                        .OrderBy(s => s.Id)
                        .ThenBy(s => s.CreatedAt),

            "status" =>
                sortDescending
                    ? query
                        .OrderByDescending(s => s.Status)
                        .ThenByDescending(s => s.Id)
                    : query
                        .OrderBy(s => s.Status)
                        .ThenBy(s => s.Id),

            "totalkobo" =>
                sortDescending
                    ? query
                        .OrderByDescending(s => s.TotalKobo)
                        .ThenByDescending(s => s.Id)
                    : query
                        .OrderBy(s => s.TotalKobo)
                        .ThenBy(s => s.Id),

            "createdat" =>
                sortDescending
                    ? query
                        .OrderByDescending(s => s.CreatedAt)
                        .ThenByDescending(s => s.Id)
                    : query
                        .OrderBy(s => s.CreatedAt)
                        .ThenBy(s => s.Id),

            _ =>
                query
                    .OrderByDescending(s => s.CreatedAt)
                    .ThenByDescending(s => s.Id)
        };
    }
}