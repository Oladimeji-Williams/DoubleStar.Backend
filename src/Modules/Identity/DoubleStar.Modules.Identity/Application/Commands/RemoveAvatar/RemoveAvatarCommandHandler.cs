// .../RemoveAvatarCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Commands.RemoveAvatarCommand;

public sealed class RemoveAvatarCommandHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<RemoveAvatarCommand, Result>
{
    public Task<Result> Handle(RemoveAvatarCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Task.FromResult(Result.Failure(UserErrors.NotAuthenticated()));
        return identityService.RemoveAvatarAsync(currentUser.UserId.Value, cancellationToken);
    }
}