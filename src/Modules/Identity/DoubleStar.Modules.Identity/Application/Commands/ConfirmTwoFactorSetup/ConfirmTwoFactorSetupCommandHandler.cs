// .../ConfirmTwoFactorSetupCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Commands.ConfirmTwoFactorSetupCommand;

public sealed class ConfirmTwoFactorSetupCommandHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<ConfirmTwoFactorSetupCommand, Result>
{
    public Task<Result> Handle(ConfirmTwoFactorSetupCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Task.FromResult(Result.Failure(UserErrors.NotAuthenticated()));
        return identityService.ConfirmTwoFactorSetupAsync(currentUser.UserId.Value, request.Code, cancellationToken);
    }
}