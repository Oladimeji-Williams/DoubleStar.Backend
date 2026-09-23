// .../SetProductPriceCommandValidator.cs
namespace DoubleStar.Modules.Catalog.Application.Commands.SetProductPriceCommand;

public sealed class SetProductPriceCommandValidator : AbstractValidator<SetProductPriceCommand>
{
    public SetProductPriceCommandValidator() => RuleFor(x => x.UnitPriceKobo).GreaterThanOrEqualTo(0);
}