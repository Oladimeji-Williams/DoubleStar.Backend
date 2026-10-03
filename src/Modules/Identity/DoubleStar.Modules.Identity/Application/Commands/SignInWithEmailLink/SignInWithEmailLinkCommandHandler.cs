// .../SignInWithEmailLinkCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Identity.Application.Errors;
using DoubleStar.Modules.Identity.Application.Mappings;

namespace DoubleStar.Modules.Identity.Application.Commands.SignInWithEmailLinkCommand;

public sealed class SignInWithEmailLinkCommandHandler(IIdentityService identityService)
    : IRequestHandler<SignInWithEmailLinkCommand, Result<Application.DTOs.AuthResultDto>>
{
    public async Task<Result<Application.DTOs.AuthResultDto>> Handle(SignInWithEmailLinkCommand request, CancellationToken cancellationToken)
    {
        var outcome = await identityService.SignInWithEmailLinkAsync(request.Token, cancellationToken);
        return outcome switch
        {
            LoginOutcome.Success success => Result<Application.DTOs.AuthResultDto>.Success(success.Result.ToDto()),
            _ => Result<Application.DTOs.AuthResultDto>.Failure(AuthErrors.InvalidTwoFactorCode()),
        };
    }
}