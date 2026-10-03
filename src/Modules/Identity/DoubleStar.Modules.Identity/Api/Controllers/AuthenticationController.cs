using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;
using DoubleStar.BuildingBlocks.Infrastructure.RateLimiting;
using DoubleStar.Modules.Identity.Api.Contracts;
using DoubleStar.Modules.Identity.Application.Commands.BeginTwoFactorSetupCommand;
using DoubleStar.Modules.Identity.Application.Commands.ChangePasswordCommand;
using DoubleStar.Modules.Identity.Application.Commands.ConfirmEmailCommand;
using DoubleStar.Modules.Identity.Application.Commands.ConfirmTwoFactorSetupCommand;
using DoubleStar.Modules.Identity.Application.Commands.DisableTwoFactorCommand;
using DoubleStar.Modules.Identity.Application.Commands.LoginCommand;
using DoubleStar.Modules.Identity.Application.Commands.RefreshTokenCommand;
using DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;
using DoubleStar.Modules.Identity.Application.Commands.RemoveAvatarCommand;
using DoubleStar.Modules.Identity.Application.Commands.RequestEmailSignInCodeCommand;
using DoubleStar.Modules.Identity.Application.Commands.RequestPasswordResetCommand;
using DoubleStar.Modules.Identity.Application.Commands.ResetPasswordCommand;
using DoubleStar.Modules.Identity.Application.Commands.RevokeRefreshTokenCommand;
using DoubleStar.Modules.Identity.Application.Commands.SignInWithEmailCodeCommand;
using DoubleStar.Modules.Identity.Application.Commands.SignInWithEmailLinkCommand;
using DoubleStar.Modules.Identity.Application.Commands.UploadAvatarCommand;
using DoubleStar.Modules.Identity.Application.Commands.UpdateMyProfileCommand;
using DoubleStar.Modules.Identity.Application.Commands.VerifyTwoFactorChallengeCommand;
using DoubleStar.Modules.Identity.Application.DTOs;
using DoubleStar.Modules.Identity.Application.Queries.GetMyProfileQuery;
using DoubleStar.SharedKernel.Contracts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DoubleStar.Modules.Identity.Api.Controllers;

public sealed class AuthenticationController(ISender sender) : V1ControllerBase
{
    [HttpPost("register-customer")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterCustomer(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RegisterCustomerCommand(
                request.Email,
                request.Password,
                request.TurnstileToken),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginCommand(
                request.EmailOrPhone,
                request.Password),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("revoke")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Revoke(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RevokeRefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangePasswordCommand(
                request.CurrentPassword,
                request.NewPassword),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetMyProfileQuery(),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateMyProfileCommand(
                request.FirstName,
                request.LastName,
                request.Phone),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpPost("me/avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Failure(
                Result<string>.Failure(
                    new Error(
                        "Avatar.Empty",
                        "No file was uploaded.",
                        ErrorType.Validation)));
        }

        await using var stream = file.OpenReadStream();

        var result = await sender.Send(
            new UploadAvatarCommand(
                stream,
                file.FileName,
                file.ContentType,
                file.Length),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpDelete("me/avatar")]
    [Authorize]
    public async Task<IActionResult> RemoveAvatar(
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RemoveAvatarCommand(),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpPost("me/two-factor/setup")]
    [Authorize]
    public async Task<IActionResult> BeginTwoFactorSetup(
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new BeginTwoFactorSetupCommand(),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("me/two-factor/confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmTwoFactorSetup(
        ConfirmTwoFactorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ConfirmTwoFactorSetupCommand(request.Code),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpPost("me/two-factor/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor(
        DisableTwoFactorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DisableTwoFactorCommand(request.CurrentPassword),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpPost("login/two-factor/verify")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    public async Task<IActionResult> VerifyTwoFactorChallenge(
        VerifyTwoFactorChallengeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new VerifyTwoFactorChallengeCommand(
                request.ChallengeToken,
                request.Code),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("login/email-code/request")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    public async Task<IActionResult> RequestEmailSignInCode(
        RequestEmailSignInCodeRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new RequestEmailSignInCodeCommand(request.Email),
            cancellationToken);

        // Always 204 — never reveal whether the email has an account.
        return NoContent();
    }

    [HttpPost("login/email-code/verify")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    public async Task<IActionResult> VerifyEmailSignInCode(
        VerifyEmailSignInCodeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SignInWithEmailCodeCommand(
                request.Email,
                request.Code),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("login/email-code/complete-link")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    public async Task<IActionResult> CompleteEmailLinkSignIn(
        CompleteEmailLinkRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SignInWithEmailLinkCommand(request.Token),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("password-reset/request")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPasswordReset(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RequestPasswordResetCommand(
                request.Email,
                request.TurnstileToken),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpPost("password-reset/confirm")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    public async Task<IActionResult> ConfirmPasswordReset(
        ConfirmPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ResetPasswordCommand(
                request.Email,
                request.Token,
                request.NewPassword),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    public async Task<IActionResult> ConfirmEmail(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ConfirmEmailCommand(
                request.Email,
                request.Token),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }
}