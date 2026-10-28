using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Abstractions.Notifications;
using DoubleStar.SharedKernel.Common;
using DoubleStar.SharedKernel.Common.Options;

using DoubleStar.Modules.Identity.Application.Abstractions;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace DoubleStar.Modules.Identity.Application.Commands.RequestPasswordResetCommand;

public sealed class RequestPasswordResetCommandHandler(
    IIdentityService identityService,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions,
    IHumanVerifier humanVerifier)
    : IRequestHandler<RequestPasswordResetCommand, Result>
{
    public async Task<Result> Handle(
        RequestPasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        if (!await humanVerifier.VerifyAsync(request.TurnstileToken, cancellationToken))
        {
            return Result.Failure(
                new Error(
                    "Verification.Required",
                    "Please complete the verification step.",
                    ErrorType.Validation));
        }

        var token = await identityService.GeneratePasswordResetTokenAsync(
            request.Email,
            cancellationToken);

        if (token is not null)
        {
            var resetLink =
                $"{frontendOptions.Value.CustomerAppUrl}/reset-password" +
                $"?email={Uri.EscapeDataString(request.Email)}" +
                $"&token={Uri.EscapeDataString(token)}";

            await emailSender.SendPasswordResetAsync(
                request.Email,
                resetLink,
                cancellationToken);
        }

        // Never reveal whether the account exists.
        return Result.Success();
    }
}