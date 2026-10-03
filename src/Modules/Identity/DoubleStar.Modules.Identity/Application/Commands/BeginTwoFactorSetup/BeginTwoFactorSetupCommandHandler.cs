// .../BeginTwoFactorSetupCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Commands.BeginTwoFactorSetupCommand;

public sealed class BeginTwoFactorSetupCommandHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<BeginTwoFactorSetupCommand, Result<TwoFactorSetupDto>>
{
    public Task<Result<TwoFactorSetupDto>> Handle(BeginTwoFactorSetupCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Task.FromResult(Result<TwoFactorSetupDto>.Failure(UserErrors.NotAuthenticated()));
        return identityService.BeginTwoFactorSetupAsync(currentUser.UserId.Value, cancellationToken);
    }
}