// RegisterCustomerCommandHandler.cs — full replacement
using Microsoft.AspNetCore.Hosting;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Identity.Infrastructure.Turnstile;
using Microsoft.Extensions.Hosting;

namespace DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;

public sealed class RegisterCustomerCommandHandler(
    IIdentityService identityService, ITurnstileVerifier turnstileVerifier, IWebHostEnvironment environment, IPublisher publisher)
    : IRequestHandler<RegisterCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment("Testing"))
        {
            if (!await turnstileVerifier.VerifyAsync(request.TurnstileToken, cancellationToken))
            {
                return Result<Guid>.Failure(new Error("Verification.Required", "Please complete the verification step.", ErrorType.Validation));
            }
        }

        var result = await identityService.RegisterCustomerAsync(request.Email, request.Password, cancellationToken);
        if (result.IsSuccess)
        {
            await publisher.Publish(new CustomerAccountRegisteredEvent(result.Value, request.Email), cancellationToken);
        }
        return result;
    }
}