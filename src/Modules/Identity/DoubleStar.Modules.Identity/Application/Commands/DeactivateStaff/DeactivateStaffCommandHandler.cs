// .../DeactivateStaffCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;

namespace DoubleStar.Modules.Identity.Application.Commands.DeactivateStaffCommand;

public sealed class DeactivateStaffCommandHandler(IIdentityService identityService)
    : IRequestHandler<DeactivateStaffCommand, Result>
{
    public Task<Result> Handle(DeactivateStaffCommand request, CancellationToken cancellationToken) =>
        identityService.DeactivateStaffAsync(request.UserId, cancellationToken);
}