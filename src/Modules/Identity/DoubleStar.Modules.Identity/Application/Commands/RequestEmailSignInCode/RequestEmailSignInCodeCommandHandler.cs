// .../RequestEmailSignInCodeCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;

namespace DoubleStar.Modules.Identity.Application.Commands.RequestEmailSignInCodeCommand;

public sealed class RequestEmailSignInCodeCommandHandler(IIdentityService identityService)
    : IRequestHandler<RequestEmailSignInCodeCommand, Result>
{
    public Task<Result> Handle(RequestEmailSignInCodeCommand request, CancellationToken cancellationToken) =>
        identityService.RequestEmailSignInCodeAsync(request.Email, cancellationToken);
}