// .../GetPendingReviewsQueryHandler.cs
using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Application.DTOs;
using DoubleStar.Modules.Reviews.Application.Mappings;

namespace DoubleStar.Modules.Reviews.Application.Queries.GetPendingReviewsQuery;

public sealed class GetPendingReviewsQueryHandler(IReviewRepository reviewRepository)
    : IRequestHandler<GetPendingReviewsQuery, Result<IReadOnlyList<ReviewDto>>>
{
    public async Task<Result<IReadOnlyList<ReviewDto>>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await reviewRepository.GetPendingAsync(cancellationToken);
        return Result<IReadOnlyList<ReviewDto>>.Success(reviews.Select(r => r.ToDto(canDelete: true)).ToList());
    }
}