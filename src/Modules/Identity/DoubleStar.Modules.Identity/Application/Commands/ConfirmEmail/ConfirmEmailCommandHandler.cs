// .../ConfirmEmailCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
namespace DoubleStar.Modules.Identity.Application.Commands.ConfirmEmailCommand;

public sealed class ConfirmEmailCommandHandler(IIdentityService identityService) : IRequestHandler<ConfirmEmailCommand, Result>
{
    public Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken) =>
        identityService.ConfirmEmailAsync(request.Email, request.Token, cancellationToken);
}