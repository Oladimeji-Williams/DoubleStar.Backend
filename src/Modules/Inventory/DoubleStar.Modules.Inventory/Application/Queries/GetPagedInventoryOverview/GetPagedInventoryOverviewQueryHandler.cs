using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Application.DTOs;
using DoubleStar.SharedKernel.Contracts.Catalog;
using MediatR;

namespace DoubleStar.Modules.Inventory.Application.Queries.GetPagedInventoryOverview;

public sealed class GetPagedInventoryOverviewQueryHandler(
    IProductCatalog productCatalog,
    IStockItemRepository stockItemRepository,
    ISerializedUnitRepository serializedUnitRepository)
    : IRequestHandler<
        GetPagedInventoryOverviewQuery,
        Result<PagedResult<InventoryOverviewDto>>>
{
    private const int LowStockThreshold = 5;

    public async Task<Result<PagedResult<InventoryOverviewDto>>> Handle(
        GetPagedInventoryOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var products =
            await productCatalog.GetAllAsync(cancellationToken);

        var stockItems =
            await stockItemRepository.GetAllAsync(cancellationToken);

        var serializedCounts =
            await serializedUnitRepository.GetInStockCountsByProductAsync(
                cancellationToken);

        var stockMap = stockItems.ToDictionary(
            stock => stock.ProductId);

        var rows = products
            .Select(product =>
            {
                stockMap.TryGetValue(
                    product.Id,
                    out var stock);

                var reserved =
                    stock?.QuantityReserved ?? 0;

                var available =
                    product.TrackingMode.ToString() == "Serialized"
                        ? serializedCounts.GetValueOrDefault(product.Id)
                        : Math.Max(
                            0,
                            (stock?.QuantityOnHand ?? 0) - reserved);

                return new InventoryOverviewDto(
                    product.Id,
                    product.Name,
                    product.Sku,
                    product.TrackingMode.ToString(),
                    available,
                    reserved,
                    GetStatus(available));
            })
            .ToList();

        rows = ApplyFilters(
            rows,
            request.Query.Filters);

        rows = ApplySorting(
            rows,
            request.Query.SortBy,
            request.Query.SortDescending);

        var totalCount = rows.Count;

        var page = request.Query.NormalizedPage;
        var pageSize = request.Query.NormalizedPageSize;

        var items = rows
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Result<PagedResult<InventoryOverviewDto>>.Success(
            new PagedResult<InventoryOverviewDto>(
                items,
                page,
                pageSize,
                totalCount));
    }

    private static string GetStatus(int availableQuantity)
    {
        if (availableQuantity <= 0)
        {
            return "Out of Stock";
        }

        if (availableQuantity <= LowStockThreshold)
        {
            return "Low Stock";
        }

        return "In Stock";
    }

    private static List<InventoryOverviewDto> ApplyFilters(
        IEnumerable<InventoryOverviewDto> source,
        IReadOnlyList<FilterCriterion>? filters)
    {
        var result = source.ToList();

        if (filters is null || filters.Count == 0)
        {
            return result;
        }

        foreach (var filter in filters)
        {
            result = result
                .Where(row => MatchesFilter(row, filter))
                .ToList();
        }

        return result;
    }

    private static bool MatchesFilter(
        InventoryOverviewDto row,
        FilterCriterion filter)
    {
        var value = GetFieldValue(row, filter.Field);

        switch (filter.Operator)
        {
            case FilterOperator.IsEmpty:
                return string.IsNullOrWhiteSpace(value);

            case FilterOperator.IsNotEmpty:
                return !string.IsNullOrWhiteSpace(value);
        }

        var filterValue = filter.Value ?? string.Empty;

        return filter.Operator switch
        {
            FilterOperator.Equals =>
                string.Equals(
                    value,
                    filterValue,
                    StringComparison.OrdinalIgnoreCase),

            FilterOperator.NotEquals =>
                !string.Equals(
                    value,
                    filterValue,
                    StringComparison.OrdinalIgnoreCase),

            FilterOperator.Contains =>
                value.Contains(
                    filterValue,
                    StringComparison.OrdinalIgnoreCase),

            FilterOperator.StartsWith =>
                value.StartsWith(
                    filterValue,
                    StringComparison.OrdinalIgnoreCase),

            FilterOperator.EndsWith =>
                value.EndsWith(
                    filterValue,
                    StringComparison.OrdinalIgnoreCase),

            FilterOperator.GreaterThan =>
                CompareValues(value, filterValue) > 0,

            FilterOperator.GreaterThanOrEqual =>
                CompareValues(value, filterValue) >= 0,

            FilterOperator.LessThan =>
                CompareValues(value, filterValue) < 0,

            FilterOperator.LessThanOrEqual =>
                CompareValues(value, filterValue) <= 0,

            _ => true
        };
    }

    private static int CompareValues(
        string value,
        string filterValue)
    {
        if (int.TryParse(value, out var numericValue) &&
            int.TryParse(filterValue, out var numericFilterValue))
        {
            return numericValue.CompareTo(numericFilterValue);
        }

        return string.Compare(
            value,
            filterValue,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string GetFieldValue(
        InventoryOverviewDto row,
        string field)
    {
        return field.ToLowerInvariant() switch
        {
            "productname" => row.ProductName,
            "sku" => row.Sku,
            "trackingmode" => row.TrackingMode,
            "availablequantity" => row.AvailableQuantity.ToString(),
            "reservedquantity" => row.ReservedQuantity.ToString(),
            "status" => row.Status,
            _ => string.Empty
        };
    }

    private static List<InventoryOverviewDto> ApplySorting(
        IEnumerable<InventoryOverviewDto> source,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "productname" => sortDescending
                ? source.OrderByDescending(x => x.ProductName).ToList()
                : source.OrderBy(x => x.ProductName).ToList(),

            "sku" => sortDescending
                ? source.OrderByDescending(x => x.Sku).ToList()
                : source.OrderBy(x => x.Sku).ToList(),

            "trackingmode" => sortDescending
                ? source.OrderByDescending(x => x.TrackingMode).ToList()
                : source.OrderBy(x => x.TrackingMode).ToList(),

            "availablequantity" => sortDescending
                ? source.OrderByDescending(x => x.AvailableQuantity).ToList()
                : source.OrderBy(x => x.AvailableQuantity).ToList(),

            "reservedquantity" => sortDescending
                ? source.OrderByDescending(x => x.ReservedQuantity).ToList()
                : source.OrderBy(x => x.ReservedQuantity).ToList(),

            "status" => sortDescending
                ? source.OrderByDescending(x => x.Status).ToList()
                : source.OrderBy(x => x.Status).ToList(),

            _ => source
                .OrderBy(x => x.ProductName)
                .ToList()
        };
    }
}