using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;
using DoubleStar.Modules.Identity.Api.Contracts;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Identity.Application.Commands.ChangePasswordCommand;
using DoubleStar.Modules.Identity.Application.Commands.LoginCommand;
using DoubleStar.Modules.Identity.Application.Commands.RefreshTokenCommand;
using DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;
using DoubleStar.Modules.Identity.Application.Commands.RevokeRefreshTokenCommand;
using DoubleStar.Modules.Identity.Application.Queries.GetMyProfileQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DoubleStar.Modules.Identity.Application.DTOs;
using DoubleStar.BuildingBlocks.Infrastructure.RateLimiting;

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
                request.FirstName,
                request.LastName,
                request.Email,
                request.Phone,
                request.Password),
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
}