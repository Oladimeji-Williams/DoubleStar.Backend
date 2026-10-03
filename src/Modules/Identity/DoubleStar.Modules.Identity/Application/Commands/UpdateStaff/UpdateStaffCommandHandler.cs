using DoubleStar.Modules.Identity.Application.Errors;
using DoubleStar.SharedKernel.Abstractions.Authentication;

namespace DoubleStar.Modules.Identity.Application.Commands.UpdateStaffCommand;

public sealed class UpdateStaffCommandHandler(IIdentityService identityService, ICurrentUser currentUser)
    : IRequestHandler<UpdateStaffCommand, Result>
{
    public Task<Result> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == request.UserId)
        {
            return Task.FromResult(Result.Failure(AuthErrors.CannotModifyOwnAccount()));
        }

        return identityService.UpdateStaffAsync(request.UserId, request.FirstName, request.LastName, request.Role, cancellationToken);
    }
}