// .../ReactivateStaffCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;

namespace DoubleStar.Modules.Identity.Application.Commands.ReactivateStaffCommand;

public sealed class ReactivateStaffCommandHandler(IIdentityService identityService)
    : IRequestHandler<ReactivateStaffCommand, Result>
{
    public Task<Result> Handle(ReactivateStaffCommand request, CancellationToken cancellationToken) =>
        identityService.ReactivateStaffAsync(request.UserId, cancellationToken);
}