// .../AdjustStockCommandValidator.cs
namespace DoubleStar.Modules.Inventory.Application.Commands.AdjustStockCommand;

public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(200);
    }
}