using System.Globalization;
using Microsoft.EntityFrameworkCore;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Persistence.Repositories;

public sealed class OrderRepository(OrdersDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Orders
            .Include(order => order.Lines)
            .Include(order => order.StatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public Task<Order?> GetByIdempotencyKeyAsync(
        Guid customerId,
        string key,
        CancellationToken cancellationToken = default) =>
        dbContext.Orders
            .Include(order => order.Lines)
            .Include(order => order.StatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                order => order.CustomerId == customerId && order.IdempotencyKey == key,
                cancellationToken);

    public async Task<IReadOnlyList<Order>> GetPendingExpiredBeforeAsync(
        DateTime cutoffUtc,
        int take,
        CancellationToken cancellationToken = default) =>
        await dbContext.Orders
            .Include(order => order.Lines)
            .Include(order => order.StatusHistory)
            .AsSplitQuery()
            .Where(order => order.Status == OrderStatus.PendingPayment && order.ExpiresAtUtc <= cutoffUtc)
            .OrderBy(order => order.ExpiresAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<PagedResult<Order>> SearchAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default) =>
        SearchAsync(query, customerId: null, cancellationToken);

    public Task<PagedResult<Order>> SearchForCustomerAsync(
        Guid customerId,
        PagedQuery query,
        CancellationToken cancellationToken = default) =>
        SearchAsync(query, customerId, cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveAsync(Order order, CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private async Task<PagedResult<Order>> SearchAsync(
        PagedQuery request,
        Guid? customerId,
        CancellationToken cancellationToken)
    {
        var page = request.NormalizedPage;
        var pageSize = request.NormalizedPageSize;

        IQueryable<Order> query = dbContext.Orders.AsNoTracking();

        if (customerId is Guid owner)
        {
            query = query.Where(order => order.CustomerId == owner);
        }

        query = ApplySearch(query, request.Search);
        query = ApplyFilters(query, request.Filters);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await ApplySorting(query, request.SortBy, request.SortDescending)
            .Include(order => order.Lines)
            .Include(order => order.StatusHistory)
            .AsSplitQuery()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>(items, page, pageSize, totalCount);
    }

    private static IQueryable<Order> ApplySearch(IQueryable<Order> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();
        var numeric = term.TrimStart('#');
        if (numeric.StartsWith("DS-", StringComparison.OrdinalIgnoreCase))
        {
            numeric = numeric[3..];
        }

        if (int.TryParse(numeric, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
        {
            return query.Where(order => order.Id == id);
        }

        var pattern = $"%{term}%";
        return query.Where(order =>
            EF.Functions.ILike(order.ContactName, pattern) ||
            EF.Functions.ILike(order.ContactPhone, pattern));
    }

    private static IQueryable<Order> ApplyFilters(
        IQueryable<Order> query,
        IReadOnlyList<FilterCriterion>? filters)
    {
        if (filters is null)
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
                case "status" when Enum.TryParse<OrderStatus>(filter.Value, true, out var status):
                    query = filter.Operator switch
                    {
                        FilterOperator.Equals => query.Where(order => order.Status == status),
                        FilterOperator.NotEquals => query.Where(order => order.Status != status),
                        _ => query,
                    };
                    break;

                case "needsattention" when bool.TryParse(filter.Value, out var needsAttention):
                    query = filter.Operator switch
                    {
                        FilterOperator.Equals => query.Where(order => order.NeedsAttention == needsAttention),
                        FilterOperator.NotEquals => query.Where(order => order.NeedsAttention != needsAttention),
                        _ => query,
                    };
                    break;
            }
        }

        return query;
    }

    private static IQueryable<Order> ApplySorting(
        IQueryable<Order> query,
        string? sortBy,
        bool descending) =>
        sortBy?.Trim().ToLowerInvariant() switch
        {
            "id" => descending ? query.OrderByDescending(order => order.Id) : query.OrderBy(order => order.Id),
            "status" => descending
                ? query.OrderByDescending(order => order.Status).ThenByDescending(order => order.Id)
                : query.OrderBy(order => order.Status).ThenBy(order => order.Id),
            "totalkobo" => descending
                ? query.OrderByDescending(order => order.TotalKobo).ThenByDescending(order => order.Id)
                : query.OrderBy(order => order.TotalKobo).ThenBy(order => order.Id),
            "contactname" => descending
                ? query.OrderByDescending(order => order.ContactName).ThenByDescending(order => order.Id)
                : query.OrderBy(order => order.ContactName).ThenBy(order => order.Id),
            _ => descending
                ? query.OrderByDescending(order => order.CreatedAt).ThenByDescending(order => order.Id)
                : query.OrderBy(order => order.CreatedAt).ThenBy(order => order.Id),
        };
}
