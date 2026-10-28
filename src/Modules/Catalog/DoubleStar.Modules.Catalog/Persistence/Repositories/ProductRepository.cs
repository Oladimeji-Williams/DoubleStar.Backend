using Microsoft.EntityFrameworkCore;

using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Domain.Entities;
using DoubleStar.SharedKernel.Contracts.Catalog;

namespace DoubleStar.Modules.Catalog.Persistence.Repositories;

public sealed class ProductRepository(
    CatalogDbContext dbContext)
    : IProductRepository
{
    public Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        dbContext.Products
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids
            .Distinct()
            .ToList();

        return await dbContext.Products
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default) =>
        dbContext.Products
            .FirstOrDefaultAsync(
                p => p.Sku == sku.ToUpperInvariant(),
                cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Products
            .Where(p => !p.IsArchived)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> SearchAsync(
        string term,
        CancellationToken cancellationToken = default)
    {
        var pattern = $"%{term}%";

        return await dbContext.Products
            .Where(p =>
                !p.IsArchived &&
                (
                    EF.Functions.ILike(p.Name, pattern) ||
                    EF.Functions.ILike(p.Sku, pattern)
                ))
            .OrderBy(p => p.Name)
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(
            product,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        dbContext.Products.Update(product);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetArchivedAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Products
            .Where(p => p.IsArchived)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<Product>> SearchAsync(
        PagedQuery request,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        var page = request.NormalizedPage;
        var pageSize = request.NormalizedPageSize;

        IQueryable<Product> query =
            dbContext.Products
                .AsNoTracking();

        if (!includeArchived)
        {
            query = query.Where(
                p => !p.IsArchived);
        }

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
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<Product>(
            items,
            page,
            pageSize,
            totalCount);
    }

    private static IQueryable<Product> ApplySearch(
        IQueryable<Product> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var searchTerm = search.Trim();
        var pattern = $"%{searchTerm}%";

        return query.Where(p =>
            EF.Functions.ILike(
                p.Name,
                pattern) ||
            EF.Functions.ILike(
                p.Sku,
                pattern));
    }

    private static IQueryable<Product> ApplyFilters(
        IQueryable<Product> query,
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
                    ApplyIdFilter(query, filter),

                "name" =>
                    ApplyNameFilter(query, filter),

                "sku" =>
                    ApplySkuFilter(query, filter),

                "categoryid" =>
                    ApplyCategoryFilter(query, filter),

                "brandid" =>
                    ApplyBrandFilter(query, filter),

                "unitpricekobo" =>
                    ApplyPriceFilter(query, filter),

                "trackingmode" =>
                    ApplyTrackingModeFilter(query, filter),

                "isarchived" =>
                    ApplyArchivedFilter(query, filter),

                _ => query
            };
        }

        return query;
    }

    private static IQueryable<Product> ApplyIdFilter(
        IQueryable<Product> query,
        FilterCriterion filter)
    {
        if (!int.TryParse(
                filter.Value,
                out var id))
        {
            return filter.Operator switch
            {
                FilterOperator.IsEmpty =>
                    query.Where(_ => false),

                FilterOperator.IsNotEmpty =>
                    query.Where(_ => true),

                _ =>
                    query.Where(_ => false)
            };
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(p => p.Id == id),

            FilterOperator.NotEquals =>
                query.Where(p => p.Id != id),

            FilterOperator.GreaterThan =>
                query.Where(p => p.Id > id),

            FilterOperator.GreaterThanOrEqual =>
                query.Where(p => p.Id >= id),

            FilterOperator.LessThan =>
                query.Where(p => p.Id < id),

            FilterOperator.LessThanOrEqual =>
                query.Where(p => p.Id <= id),

            _ => query
        };
    }

    private static IQueryable<Product> ApplyNameFilter(
        IQueryable<Product> query,
        FilterCriterion filter)
    {
        var value = filter.Value?.Trim();

        return filter.Operator switch
        {
            FilterOperator.IsEmpty =>
                query.Where(p => p.Name == string.Empty),

            FilterOperator.IsNotEmpty =>
                query.Where(p => p.Name != string.Empty),

            FilterOperator.Contains
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    EF.Functions.ILike(
                        p.Name,
                        $"%{value}%")),

            FilterOperator.StartsWith
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    EF.Functions.ILike(
                        p.Name,
                        $"{value}%")),

            FilterOperator.EndsWith
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    EF.Functions.ILike(
                        p.Name,
                        $"%{value}")),

            FilterOperator.Equals
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    EF.Functions.ILike(
                        p.Name,
                        value)),

            FilterOperator.NotEquals
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    !EF.Functions.ILike(
                        p.Name,
                        value)),

            _ => query
        };
    }

    private static IQueryable<Product> ApplySkuFilter(
        IQueryable<Product> query,
        FilterCriterion filter)
    {
        var value = filter.Value?.Trim();

        return filter.Operator switch
        {
            FilterOperator.IsEmpty =>
                query.Where(p => p.Sku == string.Empty),

            FilterOperator.IsNotEmpty =>
                query.Where(p => p.Sku != string.Empty),

            FilterOperator.Contains
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    EF.Functions.ILike(
                        p.Sku,
                        $"%{value}%")),

            FilterOperator.StartsWith
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    EF.Functions.ILike(
                        p.Sku,
                        $"{value}%")),

            FilterOperator.EndsWith
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    EF.Functions.ILike(
                        p.Sku,
                        $"%{value}")),

            FilterOperator.Equals
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    EF.Functions.ILike(
                        p.Sku,
                        value)),

            FilterOperator.NotEquals
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    !EF.Functions.ILike(
                        p.Sku,
                        value)),

            _ => query
        };
    }

    private static IQueryable<Product> ApplyCategoryFilter(
        IQueryable<Product> query,
        FilterCriterion filter)
    {
        if (filter.Operator == FilterOperator.IsEmpty)
        {
            return query.Where(
                p => p.CategoryId == null);
        }

        if (filter.Operator == FilterOperator.IsNotEmpty)
        {
            return query.Where(
                p => p.CategoryId != null);
        }

        if (!int.TryParse(
                filter.Value,
                out var categoryId))
        {
            return query.Where(_ => false);
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    p => p.CategoryId == categoryId),

            FilterOperator.NotEquals =>
                query.Where(
                    p => p.CategoryId != categoryId),

            _ => query
        };
    }

    private static IQueryable<Product> ApplyBrandFilter(
        IQueryable<Product> query,
        FilterCriterion filter)
    {
        if (filter.Operator == FilterOperator.IsEmpty)
        {
            return query.Where(
                p => p.BrandId == null);
        }

        if (filter.Operator == FilterOperator.IsNotEmpty)
        {
            return query.Where(
                p => p.BrandId != null);
        }

        if (!int.TryParse(
                filter.Value,
                out var brandId))
        {
            return query.Where(_ => false);
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    p => p.BrandId == brandId),

            FilterOperator.NotEquals =>
                query.Where(
                    p => p.BrandId != brandId),

            _ => query
        };
    }

    private static IQueryable<Product> ApplyPriceFilter(
        IQueryable<Product> query,
        FilterCriterion filter)
    {
        if (!long.TryParse(
                filter.Value,
                out var price))
        {
            return query.Where(_ => false);
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    p => p.UnitPriceKobo == price),

            FilterOperator.NotEquals =>
                query.Where(
                    p => p.UnitPriceKobo != price),

            FilterOperator.GreaterThan =>
                query.Where(
                    p => p.UnitPriceKobo > price),

            FilterOperator.GreaterThanOrEqual =>
                query.Where(
                    p => p.UnitPriceKobo >= price),

            FilterOperator.LessThan =>
                query.Where(
                    p => p.UnitPriceKobo < price),

            FilterOperator.LessThanOrEqual =>
                query.Where(
                    p => p.UnitPriceKobo <= price),

            _ => query
        };
    }

    private static IQueryable<Product> ApplyTrackingModeFilter(
        IQueryable<Product> query,
        FilterCriterion filter)
    {
        if (!Enum.TryParse<StockTrackingMode>(
                filter.Value,
                true,
                out var trackingMode))
        {
            return query.Where(_ => false);
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    p => p.TrackingMode == trackingMode),

            FilterOperator.NotEquals =>
                query.Where(
                    p => p.TrackingMode != trackingMode),

            _ => query
        };
    }

    private static IQueryable<Product> ApplyArchivedFilter(
        IQueryable<Product> query,
        FilterCriterion filter)
    {
        if (!bool.TryParse(
                filter.Value,
                out var archived))
        {
            return query.Where(_ => false);
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    p => p.IsArchived == archived),

            FilterOperator.NotEquals =>
                query.Where(
                    p => p.IsArchived != archived),

            _ => query
        };
    }

    private static IQueryable<Product> ApplySorting(
        IQueryable<Product> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "id" =>
                descending
                    ? query.OrderByDescending(p => p.Id)
                    : query.OrderBy(p => p.Id),

            "name" =>
                descending
                    ? query
                        .OrderByDescending(p => p.Name)
                        .ThenByDescending(p => p.Id)
                    : query
                        .OrderBy(p => p.Name)
                        .ThenBy(p => p.Id),

            "sku" =>
                descending
                    ? query
                        .OrderByDescending(p => p.Sku)
                        .ThenByDescending(p => p.Id)
                    : query
                        .OrderBy(p => p.Sku)
                        .ThenBy(p => p.Id),

            "categoryid" =>
                descending
                    ? query
                        .OrderByDescending(p => p.CategoryId)
                        .ThenByDescending(p => p.Id)
                    : query
                        .OrderBy(p => p.CategoryId)
                        .ThenBy(p => p.Id),

            "brandid" =>
                descending
                    ? query
                        .OrderByDescending(p => p.BrandId)
                        .ThenByDescending(p => p.Id)
                    : query
                        .OrderBy(p => p.BrandId)
                        .ThenBy(p => p.Id),

            "unitpricekobo" =>
                descending
                    ? query
                        .OrderByDescending(p => p.UnitPriceKobo)
                        .ThenByDescending(p => p.Id)
                    : query
                        .OrderBy(p => p.UnitPriceKobo)
                        .ThenBy(p => p.Id),

            "trackingmode" =>
                descending
                    ? query
                        .OrderByDescending(p => p.TrackingMode)
                        .ThenByDescending(p => p.Id)
                    : query
                        .OrderBy(p => p.TrackingMode)
                        .ThenBy(p => p.Id),

            "isarchived" =>
                descending
                    ? query
                        .OrderByDescending(p => p.IsArchived)
                        .ThenByDescending(p => p.Id)
                    : query
                        .OrderBy(p => p.IsArchived)
                        .ThenBy(p => p.Id),

            _ =>
                query
                    .OrderBy(p => p.Name)
                    .ThenBy(p => p.Id)
        };
    }
}