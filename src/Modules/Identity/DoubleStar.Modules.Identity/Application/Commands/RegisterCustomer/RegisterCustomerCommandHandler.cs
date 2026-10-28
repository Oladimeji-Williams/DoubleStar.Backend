using DoubleStar.Modules.Identity.Application.Abstractions;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;

public sealed class RegisterCustomerCommandHandler(
    IIdentityService identityService,
    IHumanVerifier humanVerifier,
    IPublisher publisher)
    : IRequestHandler<RegisterCustomerCommand, Result<RegistrationOutcome>>
{
    public async Task<Result<RegistrationOutcome>> Handle(
        RegisterCustomerCommand request,
        CancellationToken cancellationToken)
    {
        if (!await humanVerifier.VerifyAsync(request.TurnstileToken, cancellationToken))
        {
            return Result<RegistrationOutcome>.Failure(
                new Error(
                    "Verification.Required",
                    "Please complete the verification step.",
                    ErrorType.Validation));
        }

        var result = await identityService.RegisterCustomerAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (result.IsFailure)
        {
            return result;
        }

        // Registration succeeded even if the confirmation email failed, so the event must
        // still be published: the Customers module creates its Customer record from it.
        await publisher.Publish(
            new CustomerAccountRegisteredEvent(result.Value!.UserId, result.Value.Email),
            cancellationToken);

        return result;
    }
}