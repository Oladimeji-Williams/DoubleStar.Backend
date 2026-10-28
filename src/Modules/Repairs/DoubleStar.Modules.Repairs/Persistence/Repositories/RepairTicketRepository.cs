using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using DoubleStar.SharedKernel.Contracts.Repairs;

using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Domain.Entities;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;

namespace DoubleStar.Modules.Repairs.Persistence.Repositories;

public sealed class RepairTicketRepository(
    RepairsDbContext dbContext)
    : IRepairTicketRepository
{
    private IQueryable<RepairTicket> WithIncludes()
    {
        return dbContext.RepairTickets
            .AsNoTracking()
            .Include(t => t.Parts)
            .Include(t => t.StatusHistory)
            .AsSplitQuery();
    }

    public Task<RepairTicket?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        dbContext.RepairTickets
            .Include(t => t.Parts)
            .Include(t => t.StatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                t => t.Id == id,
                cancellationToken);

    public async Task<PagedResult<RepairTicket>> SearchPagedAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = query.NormalizedPage;
        var normalizedPageSize = query.NormalizedPageSize;

        IQueryable<RepairTicket> tickets = WithIncludes();

        tickets = ApplySearch(
            tickets,
            query.Search);

        tickets = ApplyFilters(
            tickets,
            query.Filters);

        tickets = ApplySorting(
            tickets,
            query.SortBy,
            query.SortDescending);

        var totalCount =
            await tickets.CountAsync(cancellationToken);

        var items =
            await tickets
                .Skip(
                    (normalizedPage - 1) *
                    normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync(cancellationToken);

        return new PagedResult<RepairTicket>(
            items,
            normalizedPage,
            normalizedPageSize,
            totalCount);
    }

    public async Task<PagedResult<RepairTicket>> SearchPagedForCustomerAsync(
        Guid customerId,
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = query.NormalizedPage;
        var normalizedPageSize = query.NormalizedPageSize;

        IQueryable<RepairTicket> tickets =
            WithIncludes()
                .Where(t => t.CustomerId == customerId);

        tickets = ApplySearch(
            tickets,
            query.Search);

        tickets = ApplyFilters(
            tickets,
            query.Filters);

        tickets = ApplySorting(
            tickets,
            query.SortBy,
            query.SortDescending);

        var totalCount =
            await tickets.CountAsync(cancellationToken);

        var items =
            await tickets
                .Skip(
                    (normalizedPage - 1) *
                    normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync(cancellationToken);

        return new PagedResult<RepairTicket>(
            items,
            normalizedPage,
            normalizedPageSize,
            totalCount);
    }

    public async Task<IReadOnlyList<RepairTicket>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await WithIncludes()
            .OrderByDescending(t => t.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RepairTicket>> GetAllForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default) =>
        await WithIncludes()
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RepairTicket>> GetByStatusAsync(
        RepairStatus status,
        CancellationToken cancellationToken = default) =>
        await WithIncludes()
            .Where(t => t.Status == status)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        RepairTicket ticket,
        CancellationToken cancellationToken = default)
    {
        await dbContext.RepairTickets.AddAsync(
            ticket,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UpdateAsync(
        RepairTicket ticket,
        CancellationToken cancellationToken = default)
    {
        dbContext.RepairTickets.Update(ticket);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<RepairTicket>> GetCollectedInRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var ticketIds =
            await dbContext.RepairStatusHistory
                .Where(h =>
                    h.ToStatus == RepairStatus.Collected &&
                    h.CreatedAt >= fromUtc &&
                    h.CreatedAt <= toUtc)
                .Select(h => h.RepairTicketId)
                .ToListAsync(cancellationToken);

        return await WithIncludes()
            .Where(t => ticketIds.Contains(t.Id))
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<RepairTicket> ApplySearch(
        IQueryable<RepairTicket> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();

        return query.Where(t =>
            t.DeviceDescription.Contains(term) ||
            (t.ImeiOrSerial != null &&
             t.ImeiOrSerial.Contains(term)) ||
            t.FaultDescription.Contains(term) ||
            (t.DiagnosisNotes != null &&
             t.DiagnosisNotes.Contains(term)));
    }

    private static IQueryable<RepairTicket> ApplyFilters(
        IQueryable<RepairTicket> query,
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

            var field =
                filter.Field.Trim().ToLowerInvariant();

            query = field switch
            {
                "id" =>
                    ApplyIdFilter(
                        query,
                        filter),

                "devicedescription" =>
                    ApplyTextFilter(
                        query,
                        t => t.DeviceDescription,
                        filter),

                "imeiorserial" =>
                    ApplyTextFilter(
                        query,
                        t => t.ImeiOrSerial,
                        filter),

                "faultdescription" =>
                    ApplyTextFilter(
                        query,
                        t => t.FaultDescription,
                        filter),

                "diagnosisnotes" =>
                    ApplyTextFilter(
                        query,
                        t => t.DiagnosisNotes,
                        filter),

                "status" =>
                    ApplyStatusFilter(
                        query,
                        filter),

                "technicianuserid" =>
                    ApplyTechnicianFilter(
                        query,
                        filter),

                "quotedpricekobo" =>
                    ApplyQuotedPriceFilter(
                        query,
                        filter),

                _ => query
            };
        }

        return query;
    }

    private static IQueryable<RepairTicket> ApplyIdFilter(
        IQueryable<RepairTicket> query,
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
                query.Where(t => t.Id == id),

            FilterOperator.NotEquals =>
                query.Where(t => t.Id != id),

            _ =>
                query
        };
    }

    private static IQueryable<RepairTicket> ApplyTextFilter(
        IQueryable<RepairTicket> query,
        Expression<Func<RepairTicket, string?>> selector,
        FilterCriterion filter)
    {
        var value = filter.Value?.Trim();

        return filter.Operator switch
        {
            FilterOperator.Contains
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(
                    BuildStringPredicate(
                        selector,
                        value!,
                        StringOperation.Contains)),

            FilterOperator.StartsWith
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(
                    BuildStringPredicate(
                        selector,
                        value!,
                        StringOperation.StartsWith)),

            FilterOperator.EndsWith
                when !string.IsNullOrWhiteSpace(value) =>
                query.Where(
                    BuildStringPredicate(
                        selector,
                        value!,
                        StringOperation.EndsWith)),

            FilterOperator.Equals
                when value is not null =>
                query.Where(
                    BuildStringPredicate(
                        selector,
                        value,
                        StringOperation.Equals)),

            FilterOperator.NotEquals
                when value is not null =>
                query.Where(
                    BuildStringPredicate(
                        selector,
                        value,
                        StringOperation.NotEquals)),

            FilterOperator.IsEmpty =>
                query.Where(
                    BuildEmptyPredicate(selector)),

            FilterOperator.IsNotEmpty =>
                query.Where(
                    BuildNotEmptyPredicate(selector)),

            _ =>
                query
        };
    }

    private static Expression<Func<RepairTicket, bool>>
        BuildStringPredicate(
            Expression<Func<RepairTicket, string?>> selector,
            string value,
            StringOperation operation)
    {
        var parameter =
            selector.Parameters[0];

        var property =
            selector.Body;

        var nullCheck =
            Expression.NotEqual(
                property,
                Expression.Constant(
                    null,
                    typeof(string)));

        var valueExpression =
            Expression.Constant(value);

        Expression comparison =
            operation switch
            {
                StringOperation.Contains =>
                    Expression.Call(
                        property,
                        nameof(string.Contains),
                        Type.EmptyTypes,
                        valueExpression),

                StringOperation.StartsWith =>
                    Expression.Call(
                        property,
                        nameof(string.StartsWith),
                        Type.EmptyTypes,
                        valueExpression),

                StringOperation.EndsWith =>
                    Expression.Call(
                        property,
                        nameof(string.EndsWith),
                        Type.EmptyTypes,
                        valueExpression),

                StringOperation.Equals =>
                    Expression.Equal(
                        property,
                        valueExpression),

                StringOperation.NotEquals =>
                    Expression.NotEqual(
                        property,
                        valueExpression),

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(operation))
            };

        var body =
            Expression.AndAlso(
                nullCheck,
                comparison);

        return Expression.Lambda<Func<RepairTicket, bool>>(
            body,
            parameter);
    }

    private static Expression<Func<RepairTicket, bool>>
        BuildEmptyPredicate(
            Expression<Func<RepairTicket, string?>> selector)
    {
        var property =
            selector.Body;

        var parameter =
            selector.Parameters[0];

        var body =
            Expression.OrElse(
                Expression.Equal(
                    property,
                    Expression.Constant(
                        null,
                        typeof(string))),

                Expression.Equal(
                    property,
                    Expression.Constant(
                        string.Empty)));

        return Expression.Lambda<Func<RepairTicket, bool>>(
            body,
            parameter);
    }

    private static Expression<Func<RepairTicket, bool>>
        BuildNotEmptyPredicate(
            Expression<Func<RepairTicket, string?>> selector)
    {
        var property =
            selector.Body;

        var parameter =
            selector.Parameters[0];

        var body =
            Expression.AndAlso(
                Expression.NotEqual(
                    property,
                    Expression.Constant(
                        null,
                        typeof(string))),

                Expression.NotEqual(
                    property,
                    Expression.Constant(
                        string.Empty)));

        return Expression.Lambda<Func<RepairTicket, bool>>(
            body,
            parameter);
    }

    private static IQueryable<RepairTicket> ApplyStatusFilter(
        IQueryable<RepairTicket> query,
        FilterCriterion filter)
    {
        if (string.IsNullOrWhiteSpace(filter.Value))
        {
            return query;
        }

        if (!Enum.TryParse<RepairStatus>(
                filter.Value,
                true,
                out var status))
        {
            return filter.Operator switch
            {
                FilterOperator.NotEquals =>
                    query.Where(_ => true),

                _ =>
                    query.Where(_ => false)
            };
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    t => t.Status == status),

            FilterOperator.NotEquals =>
                query.Where(
                    t => t.Status != status),

            _ =>
                query
        };
    }

    private static IQueryable<RepairTicket> ApplyTechnicianFilter(
        IQueryable<RepairTicket> query,
        FilterCriterion filter)
    {
        if (!Guid.TryParse(
                filter.Value,
                out var technicianId))
        {
            return filter.Operator switch
            {
                FilterOperator.IsEmpty =>
                    query.Where(
                        t => t.TechnicianUserId == null),

                FilterOperator.IsNotEmpty =>
                    query.Where(
                        t => t.TechnicianUserId != null),

                _ =>
                    query.Where(_ => false)
            };
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    t => t.TechnicianUserId == technicianId),

            FilterOperator.NotEquals =>
                query.Where(
                    t => t.TechnicianUserId != technicianId),

            FilterOperator.IsEmpty =>
                query.Where(
                    t => t.TechnicianUserId == null),

            FilterOperator.IsNotEmpty =>
                query.Where(
                    t => t.TechnicianUserId != null),

            _ =>
                query
        };
    }

    private static IQueryable<RepairTicket> ApplyQuotedPriceFilter(
        IQueryable<RepairTicket> query,
        FilterCriterion filter)
    {
        if (!long.TryParse(
                filter.Value,
                out var amount))
        {
            return filter.Operator switch
            {
                FilterOperator.IsEmpty =>
                    query.Where(
                        t => t.QuotedPriceKobo == null),

                FilterOperator.IsNotEmpty =>
                    query.Where(
                        t => t.QuotedPriceKobo != null),

                _ =>
                    query.Where(_ => false)
            };
        }

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                query.Where(
                    t => t.QuotedPriceKobo == amount),

            FilterOperator.NotEquals =>
                query.Where(
                    t => t.QuotedPriceKobo != amount),

            FilterOperator.GreaterThan =>
                query.Where(
                    t => t.QuotedPriceKobo > amount),

            FilterOperator.GreaterThanOrEqual =>
                query.Where(
                    t => t.QuotedPriceKobo >= amount),

            FilterOperator.LessThan =>
                query.Where(
                    t => t.QuotedPriceKobo < amount),

            FilterOperator.LessThanOrEqual =>
                query.Where(
                    t => t.QuotedPriceKobo <= amount),

            _ =>
                query
        };
    }

    private static IQueryable<RepairTicket> ApplySorting(
        IQueryable<RepairTicket> query,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "id" =>
                sortDescending
                    ? query
                        .OrderByDescending(t => t.Id)
                    : query
                        .OrderBy(t => t.Id),

            "devicedescription" =>
                sortDescending
                    ? query
                        .OrderByDescending(
                            t => t.DeviceDescription)
                        .ThenByDescending(
                            t => t.Id)
                    : query
                        .OrderBy(
                            t => t.DeviceDescription)
                        .ThenBy(
                            t => t.Id),

            "status" =>
                sortDescending
                    ? query
                        .OrderByDescending(
                            t => t.Status)
                        .ThenByDescending(
                            t => t.Id)
                    : query
                        .OrderBy(
                            t => t.Status)
                        .ThenBy(
                            t => t.Id),

            "quotedpricekobo" =>
                sortDescending
                    ? query
                        .OrderByDescending(
                            t => t.QuotedPriceKobo)
                        .ThenByDescending(
                            t => t.Id)
                    : query
                        .OrderBy(
                            t => t.QuotedPriceKobo)
                        .ThenBy(
                            t => t.Id),

            "technicianuserid" =>
                sortDescending
                    ? query
                        .OrderByDescending(
                            t => t.TechnicianUserId)
                        .ThenByDescending(
                            t => t.Id)
                    : query
                        .OrderBy(
                            t => t.TechnicianUserId)
                        .ThenBy(
                            t => t.Id),

            "createdat" =>
                sortDescending
                    ? query
                        .OrderByDescending(
                            t => t.CreatedAt)
                        .ThenByDescending(
                            t => t.Id)
                    : query
                        .OrderBy(
                            t => t.CreatedAt)
                        .ThenBy(
                            t => t.Id),

            _ =>
                query
                    .OrderByDescending(
                        t => t.CreatedAt)
                    .ThenByDescending(
                        t => t.Id)
        };
    }

    private enum StringOperation
    {
        Contains,
        StartsWith,
        EndsWith,
        Equals,
        NotEquals
    }
}