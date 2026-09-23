// .../LoginCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Identity.Application.Errors;
using DoubleStar.Modules.Identity.Application.Mappings;

namespace DoubleStar.Modules.Identity.Application.Commands.LoginCommand;

public sealed class LoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, Result<Application.DTOs.AuthResultDto>>
{
    public async Task<Result<Application.DTOs.AuthResultDto>> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        var outcome = await identityService.LoginAsync(request.EmailOrPhone, request.Password, cancellationToken);

        return outcome switch
        {
            LoginOutcome.Success success => Result<Application.DTOs.AuthResultDto>.Success(success.Result.ToDto()),
            LoginOutcome.AccountInactive => Result<Application.DTOs.AuthResultDto>.Failure(AuthErrors.AccountInactive()),
            LoginOutcome.AccountLockedOut lockedOut => Result<Application.DTOs.AuthResultDto>.Failure(AuthErrors.AccountLockedOut(lockedOut.LockoutEndUtc)),
            _ => Result<Application.DTOs.AuthResultDto>.Failure(AuthErrors.InvalidCredentials()),
        };
    }
}