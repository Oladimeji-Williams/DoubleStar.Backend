
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Common.Primitives;
using MediatR;

namespace DoubleStar.Modules.Identity.Application.Commands.ResendEmailConfirmationCommand;

public sealed class ResendEmailConfirmationCommandHandler(
    IIdentityService identityService)
    : IRequestHandler<ResendEmailConfirmationCommand, Result>
{
    public Task<Result> Handle(
        ResendEmailConfirmationCommand request,
        CancellationToken cancellationToken)
    {
        return identityService.ResendEmailConfirmationAsync(
            request.Email,
            cancellationToken);
    }
}