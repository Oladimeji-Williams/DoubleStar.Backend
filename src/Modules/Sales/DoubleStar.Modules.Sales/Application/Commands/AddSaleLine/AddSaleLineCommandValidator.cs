// .../AddSaleLineCommandValidator.cs
namespace DoubleStar.Modules.Sales.Application.Commands.AddSaleLineCommand;

public sealed class AddSaleLineCommandValidator : AbstractValidator<AddSaleLineCommand>
{
    public AddSaleLineCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity is not null);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
    }
}