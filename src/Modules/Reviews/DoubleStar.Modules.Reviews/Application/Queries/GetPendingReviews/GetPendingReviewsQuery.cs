using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Reviews.Application.DTOs;

namespace DoubleStar.Modules.Reviews.Application.Queries.GetPendingReviewsQuery;

public sealed record GetPendingReviewsQuery(
    PagedQuery Query)
    : IRequest<Result<PagedResult<ReviewDto>>>;