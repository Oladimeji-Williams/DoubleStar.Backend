// Identity/Application/Commands/RegisterCustomerCommand/RegisterCustomerCommandHandler.cs — replace the whole file
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;

public sealed class RegisterCustomerCommandHandler(IIdentityService identityService, IPublisher publisher)
    : IRequestHandler<RegisterCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.RegisterCustomerAsync(
            request.FirstName, request.LastName, request.Email, request.Phone, request.Password, cancellationToken);

        if (result.IsSuccess)
        {
            await publisher.Publish(
                new CustomerAccountRegisteredEvent(
                    result.Value, request.FirstName, request.LastName, request.Email, request.Phone),
                cancellationToken);
        }

        return result;
    }
}