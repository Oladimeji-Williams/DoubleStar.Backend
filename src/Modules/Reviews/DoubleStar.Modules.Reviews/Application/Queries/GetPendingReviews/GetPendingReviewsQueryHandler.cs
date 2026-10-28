using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Application.DTOs;
using DoubleStar.Modules.Reviews.Application.Mappings;

namespace DoubleStar.Modules.Reviews.Application.Queries.GetPendingReviewsQuery;

public sealed class GetPendingReviewsQueryHandler(
    IReviewRepository reviewRepository)
    : IRequestHandler<
        GetPendingReviewsQuery,
        Result<PagedResult<ReviewDto>>>
{
    public async Task<Result<PagedResult<ReviewDto>>> Handle(
        GetPendingReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await reviewRepository.GetPendingPagedAsync(
                request.Query,
                cancellationToken);

        var dtos = result.Items
            .Select(r => r.ToDto(canDelete: true))
            .ToList();

        return Result<PagedResult<ReviewDto>>.Success(
            new PagedResult<ReviewDto>(
                dtos,
                result.Page,
                result.PageSize,
                result.TotalCount));
    }
}