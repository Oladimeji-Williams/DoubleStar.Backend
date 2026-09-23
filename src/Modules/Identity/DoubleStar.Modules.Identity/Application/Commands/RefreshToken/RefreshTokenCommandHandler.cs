// .../RefreshTokenCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Identity.Application.Errors;
using DoubleStar.Modules.Identity.Application.Mappings;

namespace DoubleStar.Modules.Identity.Application.Commands.RefreshTokenCommand;

public sealed class RefreshTokenCommandHandler(IIdentityService identityService)
    : IRequestHandler<RefreshTokenCommand, Result<Application.DTOs.AuthResultDto>>
{
    public async Task<Result<Application.DTOs.AuthResultDto>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        return result is null
            ? Result<Application.DTOs.AuthResultDto>.Failure(AuthErrors.RefreshTokenInvalid())
            : Result<Application.DTOs.AuthResultDto>.Success(result.ToDto());
    }
}