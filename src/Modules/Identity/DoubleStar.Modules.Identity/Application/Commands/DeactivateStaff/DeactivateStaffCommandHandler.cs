using DoubleStar.Modules.Identity.Application.Errors;
using DoubleStar.SharedKernel.Abstractions.Authentication;

namespace DoubleStar.Modules.Identity.Application.Commands.DeactivateStaffCommand;

public sealed class DeactivateStaffCommandHandler(IIdentityService identityService, ICurrentUser currentUser)
    : IRequestHandler<DeactivateStaffCommand, Result>
{
    public Task<Result> Handle(DeactivateStaffCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == request.UserId)
        {
            return Task.FromResult(Result.Failure(AuthErrors.CannotModifyOwnAccount()));
        }

        return identityService.DeactivateStaffAsync(request.UserId, cancellationToken);
    }
}