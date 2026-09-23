// .../GetMyProfileQueryHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Queries.GetMyProfileQuery;

public sealed class GetMyProfileQueryHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<GetMyProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result<UserProfileDto>.Failure(UserErrors.NotAuthenticated());
        }

        var profile = await identityService.GetProfileAsync(currentUser.UserId.Value, cancellationToken);
        return profile is null
            ? Result<UserProfileDto>.Failure(UserErrors.NotFound(currentUser.UserId.Value))
            : Result<UserProfileDto>.Success(profile);
    }
}