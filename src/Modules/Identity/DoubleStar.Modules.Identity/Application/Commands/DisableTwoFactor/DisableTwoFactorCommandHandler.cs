// .../DisableTwoFactorCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Commands.DisableTwoFactorCommand;

public sealed class DisableTwoFactorCommandHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<DisableTwoFactorCommand, Result>
{
    public Task<Result> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Task.FromResult(Result.Failure(UserErrors.NotAuthenticated()));
        return identityService.DisableTwoFactorAsync(currentUser.UserId.Value, request.CurrentPassword, cancellationToken);
    }
}