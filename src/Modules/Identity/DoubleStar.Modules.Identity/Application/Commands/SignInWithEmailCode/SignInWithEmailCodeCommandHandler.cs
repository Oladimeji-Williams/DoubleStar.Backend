// .../SignInWithEmailCodeCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Identity.Application.Mappings;

namespace DoubleStar.Modules.Identity.Application.Commands.SignInWithEmailCodeCommand;

public sealed class SignInWithEmailCodeCommandHandler(IIdentityService identityService)
    : IRequestHandler<SignInWithEmailCodeCommand, Result<AuthenticationResult>>
{
    public async Task<Result<AuthenticationResult>> Handle(SignInWithEmailCodeCommand request, CancellationToken cancellationToken)
    {
        var outcome = await identityService.SignInWithEmailCodeAsync(request.Email, request.Code, cancellationToken);
        return outcome.ToResult();
    }
}