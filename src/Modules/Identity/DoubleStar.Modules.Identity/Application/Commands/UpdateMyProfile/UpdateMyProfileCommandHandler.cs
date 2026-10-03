// .../UpdateMyProfileCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Commands.UpdateMyProfileCommand;

public sealed class UpdateMyProfileCommandHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<UpdateMyProfileCommand, Result>
{
    public Task<Result> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Task.FromResult(Result.Failure(UserErrors.NotAuthenticated()));
        }

        return identityService.UpdateOwnProfileAsync(
            currentUser.UserId.Value, request.FirstName, request.LastName, request.Phone, cancellationToken);
    }
}