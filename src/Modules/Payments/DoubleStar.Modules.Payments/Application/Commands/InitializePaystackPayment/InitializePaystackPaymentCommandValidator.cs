using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Payments.Application.Commands.InitializePaystackPayment;

public sealed class InitializePaystackPaymentCommandValidator : AbstractValidator<InitializePaystackPaymentCommand>
{
    public InitializePaystackPaymentCommandValidator()
    {
        RuleFor(x => x.SourceType)
            .NotEqual(PaymentSourceType.Order)
            .WithMessage("Online-order payments are made by the customer at checkout and can't be started by staff.");
        RuleFor(x => x.AmountKobo).GreaterThan(0);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.CallbackUrl).NotEmpty().MaximumLength(2048);
    }
}