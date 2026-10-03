using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Identity.Application.DTOs;
using DoubleStar.Modules.Identity.Application.Errors;
using DoubleStar.Modules.Identity.Application.Mappings;

namespace DoubleStar.Modules.Identity.Application.Commands.LoginCommand;

public sealed class LoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var outcome = await identityService.LoginAsync(request.EmailOrPhone, request.Password, cancellationToken);
        return outcome switch
        {
            LoginOutcome.Success success =>
                Result<LoginResponseDto>.Success(new LoginResponseDto(false, null, success.Result.ToDto())),
            LoginOutcome.TwoFactorRequired tfa =>
                Result<LoginResponseDto>.Success(new LoginResponseDto(true, tfa.ChallengeToken, null)),
            LoginOutcome.AccountInactive =>
                Result<LoginResponseDto>.Failure(AuthErrors.AccountInactive()),
            LoginOutcome.AccountLockedOut lockedOut =>
                Result<LoginResponseDto>.Failure(AuthErrors.AccountLockedOut(lockedOut.LockoutEndUtc)),
            LoginOutcome.EmailNotConfirmed =>
                Result<LoginResponseDto>.Failure(AuthErrors.EmailNotConfirmed()),
            _ => Result<LoginResponseDto>.Failure(AuthErrors.InvalidCredentials()),
        };
    }
}