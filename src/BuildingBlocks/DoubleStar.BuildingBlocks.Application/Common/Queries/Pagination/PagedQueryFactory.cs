using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;

namespace DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;

/// <summary>Builds a <see cref="PagedQuery"/> from raw query-string values (filters arrive as JSON).</summary>
public static class PagedQueryFactory
{
    public static bool TryCreate(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        bool sortDescending,
        string? filtersJson,
        [NotNullWhen(true)] out PagedQuery? query)
    {
        IReadOnlyList<FilterCriterion>? filters = null;

        if (!string.IsNullOrWhiteSpace(filtersJson))
        {
            try
            {
                filters = JsonSerializer.Deserialize<List<FilterCriterion>>(filtersJson);
            }
            catch (JsonException)
            {
                query = null;
                return false;
            }
        }

        query = new PagedQuery(page, pageSize, search, sortBy, sortDescending, filters);
        return true;
    }
}