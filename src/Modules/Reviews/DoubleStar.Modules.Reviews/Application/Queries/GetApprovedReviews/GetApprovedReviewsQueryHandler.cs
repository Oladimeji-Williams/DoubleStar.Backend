// .../GetApprovedReviewsQueryHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Application.DTOs;
using DoubleStar.Modules.Reviews.Application.Mappings;

namespace DoubleStar.Modules.Reviews.Application.Queries.GetApprovedReviewsQuery;

public sealed class GetApprovedReviewsQueryHandler(IReviewRepository reviewRepository, ICurrentUser currentUser)
    : IRequestHandler<GetApprovedReviewsQuery, Result<IReadOnlyList<ReviewDto>>>
{
    public async Task<Result<IReadOnlyList<ReviewDto>>> Handle(GetApprovedReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await reviewRepository.GetApprovedAsync(cancellationToken);
        var dtos = reviews
            .Select(r => r.ToDto(canDelete: currentUser.IsStaff || (r.ReviewerUserId is not null && r.ReviewerUserId == currentUser.UserId)))
            .ToList();
        return Result<IReadOnlyList<ReviewDto>>.Success(dtos);
    }
}