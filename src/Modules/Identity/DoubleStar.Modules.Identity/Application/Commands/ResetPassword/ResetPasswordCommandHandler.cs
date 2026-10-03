// .../ResetPasswordCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
namespace DoubleStar.Modules.Identity.Application.Commands.ResetPasswordCommand;

public sealed class ResetPasswordCommandHandler(IIdentityService identityService) : IRequestHandler<ResetPasswordCommand, Result>
{
    public Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken) =>
        identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);
}