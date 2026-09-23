// .../RegisterStaffCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;

namespace DoubleStar.Modules.Identity.Application.Commands.RegisterStaffCommand;

public sealed class RegisterStaffCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterStaffCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(RegisterStaffCommand request, CancellationToken cancellationToken) =>
        identityService.RegisterStaffAsync(
            request.FirstName, request.LastName, request.Email, request.Password, request.Role, cancellationToken);
}