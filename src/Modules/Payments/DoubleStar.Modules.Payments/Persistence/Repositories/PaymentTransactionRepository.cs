using Microsoft.EntityFrameworkCore;

using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Domain.Entities;
using DoubleStar.Modules.Payments.Domain.Enums;

namespace DoubleStar.Modules.Payments.Persistence.Repositories;

public sealed class PaymentTransactionRepository(
    PaymentsDbContext dbContext)
    : IPaymentTransactionRepository
{
    private IQueryable<PaymentTransaction> Query() =>
        dbContext.PaymentTransactions.AsNoTracking();

    private IQueryable<PaymentTransaction> WithIncludes() =>
        dbContext.PaymentTransactions
            .Include(p => p.Refunds);

    public Task<PaymentTransaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        WithIncludes()
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);

    public Task<PaymentTransaction?> GetByPaystackReferenceAsync(
        string reference,
        CancellationToken cancellationToken = default) =>
        WithIncludes()
            .FirstOrDefaultAsync(
                p => p.PaystackReference == reference,
                cancellationToken);

    public async Task<IReadOnlyList<PaymentTransaction>> GetForSourceAsync(
        PaymentSourceType sourceType,
        int sourceId,
        CancellationToken cancellationToken = default) =>
        await WithIncludes()
            .Where(p =>
                p.SourceType == sourceType &&
                p.SourceId == sourceId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<PaymentTransaction>> GetPagedAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = Query();

        query = ApplySearch(
            query,
            request.Search);

        query = ApplyFilters(
            query,
            request.Filters);

        var totalCount =
            await query.CountAsync(cancellationToken);

        query = ApplySorting(
            query,
            request.SortBy,
            request.SortDescending);

        var page = request.NormalizedPage;
        var pageSize = request.NormalizedPageSize;

        var items =
            await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Refunds)
                .ToListAsync(cancellationToken);

        return new PagedResult<PaymentTransaction>(
            items,
            page,
            pageSize,
            totalCount);
    }

    private static IQueryable<PaymentTransaction> ApplySearch(
        IQueryable<PaymentTransaction> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();

        if (Guid.TryParse(term, out var id))
        {
            query = query.Where(p =>
                p.Id == id ||
                (p.PaystackReference != null &&
                 EF.Functions.ILike(
                     p.PaystackReference,
                     $"%{term}%")));
        }
        else if (int.TryParse(term, out var sourceId))
        {
            query = query.Where(p =>
                p.SourceId == sourceId ||
                (p.PaystackReference != null &&
                 EF.Functions.ILike(
                     p.PaystackReference,
                     $"%{term}%")));
        }
        else
        {
            query = query.Where(p =>
                (p.PaystackReference != null &&
                 EF.Functions.ILike(
                     p.PaystackReference,
                     $"%{term}%")));
        }

        return query;
    }

    private static IQueryable<PaymentTransaction> ApplyFilters(
        IQueryable<PaymentTransaction> query,
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
                filter.Field.Trim().ToLowerInvariant();

            var value =
                filter.Value?.Trim();

            switch (field)
            {
                case "sourcetype":
                    if (Enum.TryParse<PaymentSourceType>(
                        value,
                        true,
                        out var sourceType))
                    {
                        query = filter.Operator switch
                        {
                            FilterOperator.Equals =>
                                query.Where(
                                    p => p.SourceType == sourceType),

                            FilterOperator.NotEquals =>
                                query.Where(
                                    p => p.SourceType != sourceType),

                            _ => query
                        };
                    }

                    break;

                case "method":
                    if (Enum.TryParse<PaymentMethod>(
                        value,
                        true,
                        out var method))
                    {
                        query = filter.Operator switch
                        {
                            FilterOperator.Equals =>
                                query.Where(
                                    p => p.Method == method),

                            FilterOperator.NotEquals =>
                                query.Where(
                                    p => p.Method != method),

                            _ => query
                        };
                    }

                    break;

                case "status":
                    if (Enum.TryParse<PaymentStatus>(
                        value,
                        true,
                        out var status))
                    {
                        query = filter.Operator switch
                        {
                            FilterOperator.Equals =>
                                query.Where(
                                    p => p.Status == status),

                            FilterOperator.NotEquals =>
                                query.Where(
                                    p => p.Status != status),

                            _ => query
                        };
                    }

                    break;

                case "sourceid":
                    if (int.TryParse(
                        value,
                        out var sourceId))
                    {
                        query = filter.Operator switch
                        {
                            FilterOperator.Equals =>
                                query.Where(
                                    p => p.SourceId == sourceId),

                            FilterOperator.NotEquals =>
                                query.Where(
                                    p => p.SourceId != sourceId),

                            FilterOperator.GreaterThan =>
                                query.Where(
                                    p => p.SourceId > sourceId),

                            FilterOperator.GreaterThanOrEqual =>
                                query.Where(
                                    p => p.SourceId >= sourceId),

                            FilterOperator.LessThan =>
                                query.Where(
                                    p => p.SourceId < sourceId),

                            FilterOperator.LessThanOrEqual =>
                                query.Where(
                                    p => p.SourceId <= sourceId),

                            _ => query
                        };
                    }

                    break;

                case "amountkobo":
                    if (long.TryParse(
                        value,
                        out var amountKobo))
                    {
                        query = filter.Operator switch
                        {
                            FilterOperator.Equals =>
                                query.Where(
                                    p => p.AmountKobo == amountKobo),

                            FilterOperator.NotEquals =>
                                query.Where(
                                    p => p.AmountKobo != amountKobo),

                            FilterOperator.GreaterThan =>
                                query.Where(
                                    p => p.AmountKobo > amountKobo),

                            FilterOperator.GreaterThanOrEqual =>
                                query.Where(
                                    p => p.AmountKobo >= amountKobo),

                            FilterOperator.LessThan =>
                                query.Where(
                                    p => p.AmountKobo < amountKobo),

                            FilterOperator.LessThanOrEqual =>
                                query.Where(
                                    p => p.AmountKobo <= amountKobo),

                            _ => query
                        };
                    }

                    break;

                case "paystackreference":
                    query = ApplyStringFilter(
                        query,
                        filter.Operator,
                        value);

                    break;
            }
        }

        return query;
    }

    private static IQueryable<PaymentTransaction> ApplyStringFilter(
        IQueryable<PaymentTransaction> query,
        FilterOperator filterOperator,
        string? value)
    {
        return filterOperator switch
        {
            FilterOperator.Equals when value is not null =>
                query.Where(p =>
                    p.PaystackReference == value),

            FilterOperator.NotEquals when value is not null =>
                query.Where(p =>
                    p.PaystackReference != value),

            FilterOperator.Contains when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    p.PaystackReference != null &&
                    EF.Functions.ILike(
                        p.PaystackReference,
                        $"%{value}%")),

            FilterOperator.StartsWith when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    p.PaystackReference != null &&
                    EF.Functions.ILike(
                        p.PaystackReference,
                        $"{value}%")),

            FilterOperator.EndsWith when !string.IsNullOrWhiteSpace(value) =>
                query.Where(p =>
                    p.PaystackReference != null &&
                    EF.Functions.ILike(
                        p.PaystackReference,
                        $"%{value}")),

            FilterOperator.IsEmpty =>
                query.Where(p =>
                    p.PaystackReference == null ||
                    p.PaystackReference == ""),

            FilterOperator.IsNotEmpty =>
                query.Where(p =>
                    p.PaystackReference != null &&
                    p.PaystackReference != ""),

            _ => query
        };
    }

    private static IQueryable<PaymentTransaction> ApplySorting(
        IQueryable<PaymentTransaction> query,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "sourcetype" => sortDescending
                ? query.OrderByDescending(p => p.SourceType)
                    .ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.SourceType)
                    .ThenBy(p => p.Id),

            "sourceid" => sortDescending
                ? query.OrderByDescending(p => p.SourceId)
                    .ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.SourceId)
                    .ThenBy(p => p.Id),

            "amountkobo" => sortDescending
                ? query.OrderByDescending(p => p.AmountKobo)
                    .ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.AmountKobo)
                    .ThenBy(p => p.Id),

            "method" => sortDescending
                ? query.OrderByDescending(p => p.Method)
                    .ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.Method)
                    .ThenBy(p => p.Id),

            "status" => sortDescending
                ? query.OrderByDescending(p => p.Status)
                    .ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.Status)
                    .ThenBy(p => p.Id),

            "paystackreference" => sortDescending
                ? query.OrderByDescending(p => p.PaystackReference)
                    .ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.PaystackReference)
                    .ThenBy(p => p.Id),

            "createdat" => sortDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.CreatedAt)
                    .ThenBy(p => p.Id),

            _ =>
                query
                    .OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.Id)
        };
    }

    public async Task AddAsync(
        PaymentTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        await dbContext.PaymentTransactions.AddAsync(
            transaction,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UpdateAsync(
        PaymentTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        dbContext.PaymentTransactions.Update(transaction);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentMethodSummaryDto>>
        GetPaymentMethodSummaryAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken cancellationToken = default)
    {
        var summaries = await dbContext.PaymentTransactions
            .AsNoTracking()
            .Where(payment =>
                payment.CreatedAt >= fromUtc &&
                payment.CreatedAt <= toUtc &&
                payment.Status != PaymentStatus.Pending &&
                payment.Status != PaymentStatus.Failed)
            .GroupBy(payment => payment.Method)
            .Select(group => new
            {
                Method = group.Key,
                Count = group.Count(),
                AmountKobo = group.Sum(payment => payment.AmountKobo)
            })
            .OrderByDescending(x => x.AmountKobo)
            .ToListAsync(cancellationToken);

        return summaries
            .Select(x =>
                new PaymentMethodSummaryDto(
                    x.Method.ToString(),
                    x.Count,
                    x.AmountKobo))
            .ToList();
    }
}