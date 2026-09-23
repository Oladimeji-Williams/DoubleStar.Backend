// .../CreateSaleCommandValidator.cs
namespace DoubleStar.Modules.Sales.Application.Commands.CreateSaleCommand;

public sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.WalkInName).MaximumLength(200);
        RuleFor(x => x.WalkInPhone).MaximumLength(20);
    }
}