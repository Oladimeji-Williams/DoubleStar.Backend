// .../RefundPaymentCommandValidator.cs
namespace DoubleStar.Modules.Payments.Application.Commands.RefundPaymentCommand;

public sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.AmountKobo).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}