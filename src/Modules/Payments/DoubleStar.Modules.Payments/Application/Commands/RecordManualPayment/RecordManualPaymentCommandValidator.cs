// .../RecordManualPaymentCommandValidator.cs
namespace DoubleStar.Modules.Payments.Application.Commands.RecordManualPaymentCommand;

public sealed class RecordManualPaymentCommandValidator : AbstractValidator<RecordManualPaymentCommand>
{
    public RecordManualPaymentCommandValidator()
    {
        RuleFor(x => x.AmountKobo).GreaterThan(0);
        RuleFor(x => x.Method).IsInEnum();
    }
}