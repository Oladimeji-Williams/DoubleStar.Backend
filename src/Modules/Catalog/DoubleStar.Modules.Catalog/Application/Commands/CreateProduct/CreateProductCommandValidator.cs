// .../CreateProductCommandValidator.cs
namespace DoubleStar.Modules.Catalog.Application.Commands.CreateProductCommand;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.UnitPriceKobo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TrackingMode).IsInEnum();
    }
}