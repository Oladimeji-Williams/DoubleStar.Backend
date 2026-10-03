namespace DoubleStar.Modules.Identity.Application.Commands.VerifyTwoFactorChallengeCommand;

public sealed class VerifyTwoFactorChallengeCommandValidator : AbstractValidator<VerifyTwoFactorChallengeCommand>
{
    public VerifyTwoFactorChallengeCommandValidator()
    {
        RuleFor(x => x.ChallengeToken).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}