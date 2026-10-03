// Modules/Reviews/Application/Queries/GetPendingReviewsQuery/GetPendingReviewsQuery.cs
using DoubleStar.Modules.Reviews.Application.DTOs;
namespace DoubleStar.Modules.Reviews.Application.Queries.GetPendingReviewsQuery;
public sealed record GetPendingReviewsQuery : IRequest<Result<IReadOnlyList<ReviewDto>>>;