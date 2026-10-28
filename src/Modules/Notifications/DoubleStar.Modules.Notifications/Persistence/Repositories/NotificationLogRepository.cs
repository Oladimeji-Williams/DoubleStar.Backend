using Microsoft.EntityFrameworkCore;

using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Notifications.Application.Abstractions;
using DoubleStar.Modules.Notifications.Domain.Entities;
using DoubleStar.Modules.Notifications.Domain.Enums;

namespace DoubleStar.Modules.Notifications.Persistence.Repositories;

public sealed class NotificationLogRepository(
    NotificationsDbContext dbContext)
    : INotificationLogRepository
{
    public async Task AddAsync(
        NotificationLog log,
        CancellationToken cancellationToken = default)
    {
        await dbContext.NotificationLogs.AddAsync(
            log,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<PagedResult<NotificationLog>> GetPagedAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default)
    {
        var query =
            dbContext.NotificationLogs
                .AsNoTracking();

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
                .ToListAsync(cancellationToken);

        return new PagedResult<NotificationLog>(
            items,
            page,
            pageSize,
            totalCount);
    }

    private static IQueryable<NotificationLog> ApplySearch(
        IQueryable<NotificationLog> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();

        query = query.Where(log =>
            EF.Functions.ILike(
                log.Recipient,
                $"%{term}%") ||

            EF.Functions.ILike(
                log.TemplateName,
                $"%{term}%") ||

            (log.ErrorMessage != null &&
             EF.Functions.ILike(
                 log.ErrorMessage,
                 $"%{term}%")));

        return query;
    }

    private static IQueryable<NotificationLog> ApplyFilters(
        IQueryable<NotificationLog> query,
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
                case "id":
                    if (int.TryParse(
                        value,
                        out var id))
                    {
                        query = filter.Operator switch
                        {
                            FilterOperator.Equals =>
                                query.Where(
                                    n => n.Id == id),

                            FilterOperator.NotEquals =>
                                query.Where(
                                    n => n.Id != id),

                            FilterOperator.GreaterThan =>
                                query.Where(
                                    n => n.Id > id),

                            FilterOperator.GreaterThanOrEqual =>
                                query.Where(
                                    n => n.Id >= id),

                            FilterOperator.LessThan =>
                                query.Where(
                                    n => n.Id < id),

                            FilterOperator.LessThanOrEqual =>
                                query.Where(
                                    n => n.Id <= id),

                            _ => query
                        };
                    }

                    break;

                case "channel":
                    if (Enum.TryParse<NotificationChannel>(
                        value,
                        true,
                        out var channel))
                    {
                        query = filter.Operator switch
                        {
                            FilterOperator.Equals =>
                                query.Where(
                                    n => n.Channel == channel),

                            FilterOperator.NotEquals =>
                                query.Where(
                                    n => n.Channel != channel),

                            _ => query
                        };
                    }

                    break;

                case "recipient":
                    query = ApplyStringFilter(
                        query,
                        n => n.Recipient,
                        filter.Operator,
                        value);

                    break;

                case "templatename":
                    query = ApplyStringFilter(
                        query,
                        n => n.TemplateName,
                        filter.Operator,
                        value);

                    break;

                case "status":
                    if (Enum.TryParse<NotificationStatus>(
                        value,
                        true,
                        out var status))
                    {
                        query = filter.Operator switch
                        {
                            FilterOperator.Equals =>
                                query.Where(
                                    n => n.Status == status),

                            FilterOperator.NotEquals =>
                                query.Where(
                                    n => n.Status != status),

                            _ => query
                        };
                    }

                    break;

                case "errormessage":
                    query = ApplyStringFilter(
                        query,
                        n => n.ErrorMessage,
                        filter.Operator,
                        value);

                    break;
            }
        }

        return query;
    }

    private static IQueryable<NotificationLog> ApplyStringFilter(
        IQueryable<NotificationLog> query,
        System.Linq.Expressions.Expression<
            Func<NotificationLog, string?>> selector,
        FilterOperator filterOperator,
        string? value)
    {
        return filterOperator switch
        {
            FilterOperator.Equals when value is not null =>
                query.Where(
                    BuildStringPredicate(
                        selector,
                        value,
                        StringOperation.Equals)),

            FilterOperator.NotEquals when value is not null =>
                query.Where(
                    BuildStringPredicate(
                        selector,
                        value,
                        StringOperation.NotEquals)),

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

            FilterOperator.IsEmpty =>
                query.Where(
                    BuildEmptyPredicate(selector)),

            FilterOperator.IsNotEmpty =>
                query.Where(
                    BuildNotEmptyPredicate(selector)),

            _ => query
        };
    }

    private static System.Linq.Expressions.Expression<
        Func<NotificationLog, bool>> BuildStringPredicate(
        System.Linq.Expressions.Expression<
            Func<NotificationLog, string?>> selector,
        string value,
        StringOperation operation)
    {
        var parameter = selector.Parameters[0];
        var property = selector.Body;

        var valueExpression =
            System.Linq.Expressions.Expression.Constant(
                value);

        var nullCheck =
            System.Linq.Expressions.Expression.NotEqual(
                property,
                System.Linq.Expressions.Expression.Constant(
                    null,
                    typeof(string)));

        System.Linq.Expressions.Expression comparison =
            operation switch
            {
                StringOperation.Contains =>
                    System.Linq.Expressions.Expression.Call(
                        property,
                        nameof(string.Contains),
                        Type.EmptyTypes,
                        valueExpression),

                StringOperation.StartsWith =>
                    System.Linq.Expressions.Expression.Call(
                        property,
                        nameof(string.StartsWith),
                        Type.EmptyTypes,
                        valueExpression),

                StringOperation.EndsWith =>
                    System.Linq.Expressions.Expression.Call(
                        property,
                        nameof(string.EndsWith),
                        Type.EmptyTypes,
                        valueExpression),

                StringOperation.Equals =>
                    System.Linq.Expressions.Expression.Equal(
                        property,
                        valueExpression),

                StringOperation.NotEquals =>
                    System.Linq.Expressions.Expression.NotEqual(
                        property,
                        valueExpression),

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(operation))
            };

        var body =
            System.Linq.Expressions.Expression.AndAlso(
                nullCheck,
                comparison);

        return System.Linq.Expressions.Expression.Lambda<
            Func<NotificationLog, bool>>(
            body,
            parameter);
    }

    private static System.Linq.Expressions.Expression<
        Func<NotificationLog, bool>> BuildEmptyPredicate(
        System.Linq.Expressions.Expression<
            Func<NotificationLog, string?>> selector)
    {
        var property = selector.Body;
        var parameter = selector.Parameters[0];

        var body =
            System.Linq.Expressions.Expression.OrElse(
                System.Linq.Expressions.Expression.Equal(
                    property,
                    System.Linq.Expressions.Expression.Constant(
                        null,
                        typeof(string))),
                System.Linq.Expressions.Expression.Equal(
                    property,
                    System.Linq.Expressions.Expression.Constant(
                        string.Empty)));

        return System.Linq.Expressions.Expression.Lambda<
            Func<NotificationLog, bool>>(
            body,
            parameter);
    }

    private static System.Linq.Expressions.Expression<
        Func<NotificationLog, bool>> BuildNotEmptyPredicate(
        System.Linq.Expressions.Expression<
            Func<NotificationLog, string?>> selector)
    {
        var property = selector.Body;
        var parameter = selector.Parameters[0];

        var body =
            System.Linq.Expressions.Expression.AndAlso(
                System.Linq.Expressions.Expression.NotEqual(
                    property,
                    System.Linq.Expressions.Expression.Constant(
                        null,
                        typeof(string))),
                System.Linq.Expressions.Expression.NotEqual(
                    property,
                    System.Linq.Expressions.Expression.Constant(
                        string.Empty)));

        return System.Linq.Expressions.Expression.Lambda<
            Func<NotificationLog, bool>>(
            body,
            parameter);
    }

    private static IQueryable<NotificationLog> ApplySorting(
        IQueryable<NotificationLog> query,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "id" => sortDescending
                ? query.OrderByDescending(n => n.Id)
                    .ThenByDescending(n => n.CreatedAt)
                : query.OrderBy(n => n.Id)
                    .ThenBy(n => n.CreatedAt),

            "channel" => sortDescending
                ? query.OrderByDescending(n => n.Channel)
                    .ThenByDescending(n => n.Id)
                : query.OrderBy(n => n.Channel)
                    .ThenBy(n => n.Id),

            "recipient" => sortDescending
                ? query.OrderByDescending(n => n.Recipient)
                    .ThenByDescending(n => n.Id)
                : query.OrderBy(n => n.Recipient)
                    .ThenBy(n => n.Id),

            "templatename" => sortDescending
                ? query.OrderByDescending(n => n.TemplateName)
                    .ThenByDescending(n => n.Id)
                : query.OrderBy(n => n.TemplateName)
                    .ThenBy(n => n.Id),

            "status" => sortDescending
                ? query.OrderByDescending(n => n.Status)
                    .ThenByDescending(n => n.Id)
                : query.OrderBy(n => n.Status)
                    .ThenBy(n => n.Id),

            "createdat" => sortDescending
                ? query.OrderByDescending(n => n.CreatedAt)
                    .ThenByDescending(n => n.Id)
                : query.OrderBy(n => n.CreatedAt)
                    .ThenBy(n => n.Id),

            _ =>
                query
                    .OrderByDescending(n => n.CreatedAt)
                    .ThenByDescending(n => n.Id)
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