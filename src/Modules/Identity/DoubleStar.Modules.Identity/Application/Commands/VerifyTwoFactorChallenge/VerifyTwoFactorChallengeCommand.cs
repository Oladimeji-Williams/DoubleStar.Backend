// Identity/Application/Commands/VerifyTwoFactorChallengeCommand/VerifyTwoFactorChallengeCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.VerifyTwoFactorChallengeCommand;
public sealed record VerifyTwoFactorChallengeCommand(string ChallengeToken, string Code) : IRequest<Result<Application.DTOs.AuthResultDto>>;