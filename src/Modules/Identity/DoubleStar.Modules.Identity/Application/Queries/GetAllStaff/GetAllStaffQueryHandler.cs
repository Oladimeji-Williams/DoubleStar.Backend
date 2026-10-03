// .../GetAllStaffQueryHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Queries.GetAllStaffQuery;

public sealed class GetAllStaffQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetAllStaffQuery, Result<IReadOnlyList<UserProfileDto>>>
{
    public async Task<Result<IReadOnlyList<UserProfileDto>>> Handle(GetAllStaffQuery request, CancellationToken cancellationToken)
    {
        var allUsers = await identityService.GetAllUsersAsync(cancellationToken);
        var staff = allUsers.Where(u => u.Roles.Any(r => r != "Customer")).ToList();
        return Result<IReadOnlyList<UserProfileDto>>.Success(staff);
    }
}