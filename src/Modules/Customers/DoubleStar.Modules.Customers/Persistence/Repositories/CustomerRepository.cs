using Microsoft.EntityFrameworkCore;

using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.Modules.Customers.Persistence.Repositories;

public sealed class CustomerRepository(
    CustomersDbContext dbContext)
    : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);

    public Task<Customer?> GetByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default) =>
        dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Phone == phone,
                cancellationToken);

    public async Task<PagedResult<Customer>> SearchAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default)
    {
        var page = request.NormalizedPage;
        var pageSize = request.NormalizedPageSize;

        IQueryable<Customer> query =
            dbContext.Customers.AsNoTracking();

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
                .ToListAsync(cancellationToken);

        return new PagedResult<Customer>(
            items,
            page,
            pageSize,
            totalCount);
    }

    private static IQueryable<Customer> ApplySearch(
        IQueryable<Customer> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var searchTerm = search.Trim();
        var pattern = $"%{searchTerm}%";

        return query.Where(c =>
            (c.Name != null &&
             EF.Functions.ILike(
                 c.Name,
                 pattern)) ||

            (c.Phone != null &&
             EF.Functions.ILike(
                 c.Phone,
                 pattern)) ||

            (c.Email != null &&
             EF.Functions.ILike(
                 c.Email,
                 pattern)));
    }

    private static IQueryable<Customer> ApplyFilters(
        IQueryable<Customer> query,
        IReadOnlyList<FilterCriterion>? filters)
    {
        if (filters is null || filters.Count == 0)
        {
            return query;
        }

        foreach (var filter in filters)
        {
            query = ApplyFilter(
                query,
                filter);
        }

        return query;
    }

    private static IQueryable<Customer> ApplyFilter(
        IQueryable<Customer> query,
        FilterCriterion filter)
    {
        var field =
            filter.Field
                .Trim()
                .ToLowerInvariant();

        return field switch
        {
            "name" =>
                ApplyNameFilter(
                    query,
                    filter),

            "phone" =>
                ApplyPhoneFilter(
                    query,
                    filter),

            "email" =>
                ApplyEmailFilter(
                    query,
                    filter),

            "hasaccount" =>
                ApplyHasAccountFilter(
                    query,
                    filter),

            _ => query
        };
    }

    private static IQueryable<Customer> ApplyNameFilter(
        IQueryable<Customer> query,
        FilterCriterion filter)
    {
        var value = filter.Value?.Trim();

        return filter.Operator switch
        {
            FilterOperator.Contains when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Name != null &&
                    EF.Functions.ILike(
                        c.Name,
                        $"%{value}%")),

            FilterOperator.StartsWith when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Name != null &&
                    EF.Functions.ILike(
                        c.Name,
                        $"{value}%")),

            FilterOperator.EndsWith when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Name != null &&
                    EF.Functions.ILike(
                        c.Name,
                        $"%{value}")),

            FilterOperator.Equals when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Name != null &&
                    EF.Functions.ILike(
                        c.Name,
                        value)),

            FilterOperator.NotEquals when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Name == null ||
                    !EF.Functions.ILike(
                        c.Name,
                        value)),

            _ => query
        };
    }

    private static IQueryable<Customer> ApplyPhoneFilter(
        IQueryable<Customer> query,
        FilterCriterion filter)
    {
        var value = filter.Value?.Trim();

        return filter.Operator switch
        {
            FilterOperator.Contains when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Phone != null &&
                    EF.Functions.ILike(
                        c.Phone,
                        $"%{value}%")),

            FilterOperator.StartsWith when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Phone != null &&
                    EF.Functions.ILike(
                        c.Phone,
                        $"{value}%")),

            FilterOperator.EndsWith when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Phone != null &&
                    EF.Functions.ILike(
                        c.Phone,
                        $"%{value}")),

            FilterOperator.Equals when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Phone == value),

            FilterOperator.NotEquals when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Phone != value),

            FilterOperator.IsEmpty =>
                query.Where(c =>
                    c.Phone == null ||
                    c.Phone == ""),

            FilterOperator.IsNotEmpty =>
                query.Where(c =>
                    c.Phone != null &&
                    c.Phone != ""),

            _ => query
        };
    }

    private static IQueryable<Customer> ApplyEmailFilter(
        IQueryable<Customer> query,
        FilterCriterion filter)
    {
        var value = filter.Value?.Trim();

        return filter.Operator switch
        {
            FilterOperator.Contains when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Email != null &&
                    EF.Functions.ILike(
                        c.Email,
                        $"%{value}%")),

            FilterOperator.StartsWith when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Email != null &&
                    EF.Functions.ILike(
                        c.Email,
                        $"{value}%")),

            FilterOperator.EndsWith when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Email != null &&
                    EF.Functions.ILike(
                        c.Email,
                        $"%{value}")),

            FilterOperator.Equals when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Email != null &&
                    EF.Functions.ILike(
                        c.Email,
                        value)),

            FilterOperator.NotEquals when
                !string.IsNullOrWhiteSpace(value) =>
                query.Where(c =>
                    c.Email == null ||
                    !EF.Functions.ILike(
                        c.Email,
                        value)),

            FilterOperator.IsEmpty =>
                query.Where(c =>
                    c.Email == null ||
                    c.Email == ""),

            FilterOperator.IsNotEmpty =>
                query.Where(c =>
                    c.Email != null &&
                    c.Email != ""),

            _ => query
        };
    }

    private static IQueryable<Customer> ApplyHasAccountFilter(
        IQueryable<Customer> query,
        FilterCriterion filter)
    {
        if (!bool.TryParse(
                filter.Value,
                out var value))
        {
            return query;
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(c =>
                    c.HasAccount == value),

            FilterOperator.NotEquals =>
                query.Where(c =>
                    c.HasAccount != value),

            _ => query
        };
    }

    private static IQueryable<Customer> ApplySorting(
        IQueryable<Customer> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?
            .Trim()
            .ToLowerInvariant() switch
        {
            "name" => descending
                ? query
                    .OrderByDescending(c => c.Name)
                    .ThenBy(c => c.Id)
                : query
                    .OrderBy(c => c.Name)
                    .ThenBy(c => c.Id),

            "phone" => descending
                ? query
                    .OrderByDescending(c => c.Phone)
                    .ThenBy(c => c.Id)
                : query
                    .OrderBy(c => c.Phone)
                    .ThenBy(c => c.Id),

            "email" => descending
                ? query
                    .OrderByDescending(c => c.Email)
                    .ThenBy(c => c.Id)
                : query
                    .OrderBy(c => c.Email)
                    .ThenBy(c => c.Id),

            "hasaccount" => descending
                ? query
                    .OrderByDescending(c => c.HasAccount)
                    .ThenBy(c => c.Id)
                : query
                    .OrderBy(c => c.HasAccount)
                    .ThenBy(c => c.Id),

            _ => query
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Id)
        };
    }

    public async Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Customers.AddAsync(
            customer,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UpdateAsync(
        Customer customer,
        CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Update(customer);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}