// .../RevokeRefreshTokenCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Identity.Application.Errors;

namespace DoubleStar.Modules.Identity.Application.Commands.RevokeRefreshTokenCommand;

public sealed class RevokeRefreshTokenCommandHandler(IIdentityService identityService)
    : IRequestHandler<RevokeRefreshTokenCommand, Result>
{
    public async Task<Result> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var revoked = await identityService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return revoked ? Result.Success() : Result.Failure(AuthErrors.RefreshTokenInvalid());
    }
}