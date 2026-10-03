// .../VerifyTwoFactorChallengeCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Identity.Application.Errors;
using DoubleStar.Modules.Identity.Application.Mappings;

namespace DoubleStar.Modules.Identity.Application.Commands.VerifyTwoFactorChallengeCommand;

public sealed class VerifyTwoFactorChallengeCommandHandler(IIdentityService identityService)
    : IRequestHandler<VerifyTwoFactorChallengeCommand, Result<Application.DTOs.AuthResultDto>>
{
    public async Task<Result<Application.DTOs.AuthResultDto>> Handle(
        VerifyTwoFactorChallengeCommand request, CancellationToken cancellationToken)
    {
        var outcome = await identityService.VerifyTwoFactorChallengeAsync(request.ChallengeToken, request.Code, cancellationToken);
        return outcome switch
        {
            LoginOutcome.Success success => Result<Application.DTOs.AuthResultDto>.Success(success.Result.ToDto()),
            _ => Result<Application.DTOs.AuthResultDto>.Failure(AuthErrors.InvalidTwoFactorCode()),
        };
    }
}