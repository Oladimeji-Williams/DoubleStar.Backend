// Identity/Api/Controllers/HumanVerificationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.BuildingBlocks.Infrastructure.RateLimiting;
using DoubleStar.SharedKernel.Abstractions.Security;

namespace DoubleStar.Modules.Identity.Api.Controllers;

public sealed record VerifyHumanChallengeRequest(Guid ChallengeId, int SelectedIndex);

[Route("api/v1/verification")]
public sealed class HumanVerificationController(IHumanVerificationService verificationService) : V1ControllerBase
{
    [HttpPost("challenge")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    public async Task<IActionResult> CreateChallenge(CancellationToken cancellationToken)
    {
        var (id, iconCount, correctIndex) = await verificationService.CreateChallengeAsync(cancellationToken);
        return Success(new { challengeId = id, iconCount, correctIndex });
    }

    [HttpPost("verify")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
    public async Task<IActionResult> Verify(VerifyHumanChallengeRequest request, CancellationToken cancellationToken)
    {
        var success = await verificationService.VerifyAsync(request.ChallengeId, request.SelectedIndex, cancellationToken);
        return Success(new { success });
    }
}