using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Payments.Application.Commands.RecordManualPayment;

public sealed class RecordManualPaymentCommandValidator : AbstractValidator<RecordManualPaymentCommand>
{
    public RecordManualPaymentCommandValidator()
    {
        RuleFor(x => x.SourceType)
            .NotEqual(PaymentSourceType.Order)
            .WithMessage("Online-order payments are made by the customer at checkout and can't be recorded by staff.");
        RuleFor(x => x.AmountKobo).GreaterThan(0);
        RuleFor(x => x.Method).IsInEnum();
    }
}