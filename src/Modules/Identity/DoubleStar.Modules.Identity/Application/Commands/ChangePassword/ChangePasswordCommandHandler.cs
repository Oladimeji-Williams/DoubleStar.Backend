// .../ChangePasswordCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Commands.ChangePasswordCommand;

public sealed class ChangePasswordCommandHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Task.FromResult(Result.Failure(UserErrors.NotAuthenticated()));
        }

        return identityService.ChangePasswordAsync(
            currentUser.UserId.Value, request.CurrentPassword, request.NewPassword, cancellationToken);
    }
}