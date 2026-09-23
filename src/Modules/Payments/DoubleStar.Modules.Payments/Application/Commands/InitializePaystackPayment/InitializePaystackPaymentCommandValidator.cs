// .../InitializePaystackPaymentCommandValidator.cs
namespace DoubleStar.Modules.Payments.Application.Commands.InitializePaystackPaymentCommand;

public sealed class InitializePaystackPaymentCommandValidator : AbstractValidator<InitializePaystackPaymentCommand>
{
    public InitializePaystackPaymentCommandValidator()
    {
        RuleFor(x => x.AmountKobo).GreaterThan(0);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.CallbackUrl).NotEmpty().MaximumLength(2048);
    }
}