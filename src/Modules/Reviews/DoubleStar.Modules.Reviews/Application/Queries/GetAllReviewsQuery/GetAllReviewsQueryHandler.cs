using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Application.DTOs;

namespace DoubleStar.Modules.Reviews.Application.Queries.GetAllReviewsQuery;

public sealed class GetAllReviewsQueryHandler(
    IReviewRepository reviewRepository,
    IIdentityService identityService)
    : IRequestHandler<
        GetAllReviewsQuery,
        Result<PagedResult<ReviewDto>>>
{
    public async Task<Result<PagedResult<ReviewDto>>> Handle(
        GetAllReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await reviewRepository.GetAllPagedAsync(
                request.Query,
                cancellationToken);

        var actorIds = result.Items
            .SelectMany(r =>
                new[]
                {
                    r.ApprovedByUserId,
                    r.RejectedByUserId,
                    r.DeletedByUserId
                })
            .Where(id => id is not null)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var namesByActorId = new Dictionary<Guid, string>();

        foreach (var actorId in actorIds)
        {
            var profile =
                await identityService.GetProfileAsync(
                    actorId,
                    cancellationToken);

            namesByActorId[actorId] =
                profile is null
                    ? "Unknown"
                    : $"{profile.FirstName} {profile.LastName}".Trim();
        }

        string? NameOf(Guid? id) =>
            id is null
                ? null
                : namesByActorId.GetValueOrDefault(
                    id.Value,
                    "Unknown");

        var dtos = result.Items
            .Select(r => new ReviewDto(
                r.Id,
                r.ReviewerName,
                r.IsAnonymous,
                r.Rating,
                r.Comment,
                r.Status.ToString(),
                r.CreatedAt,
                CanDelete: true,
                r.IsDeleted,
                r.DeletedAt,
                NameOf(r.DeletedByUserId),
                r.ApprovedAtUtc,
                NameOf(r.ApprovedByUserId),
                r.RejectedAtUtc,
                NameOf(r.RejectedByUserId)))
            .ToList();

        return Result<PagedResult<ReviewDto>>.Success(
            new PagedResult<ReviewDto>(
                dtos,
                result.Page,
                result.PageSize,
                result.TotalCount));
    }
}