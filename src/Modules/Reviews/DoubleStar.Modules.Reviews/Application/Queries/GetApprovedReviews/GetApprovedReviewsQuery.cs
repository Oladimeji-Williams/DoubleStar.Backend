// Modules/Reviews/Application/Queries/GetApprovedReviewsQuery/GetApprovedReviewsQuery.cs
using DoubleStar.Modules.Reviews.Application.DTOs;
namespace DoubleStar.Modules.Reviews.Application.Queries.GetApprovedReviewsQuery;
public sealed record GetApprovedReviewsQuery : IRequest<Result<IReadOnlyList<ReviewDto>>>;