using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Reviews.Application.DTOs;

namespace DoubleStar.Modules.Reviews.Application.Queries.GetAllReviewsQuery;

public sealed record GetAllReviewsQuery(
    PagedQuery Query)
    : IRequest<Result<PagedResult<ReviewDto>>>;