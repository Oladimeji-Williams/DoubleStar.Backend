using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;

namespace DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;

public sealed record PagedQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    string? SortBy = null,
    bool SortDescending = false,
    IReadOnlyList<FilterCriterion>? Filters = null)
{
    public int NormalizedPage =>
        Page < 1 ? 1 : Page;

    public int NormalizedPageSize =>
        Math.Clamp(PageSize, 1, 100);
}