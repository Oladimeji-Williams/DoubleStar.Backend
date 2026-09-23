// .../RestockBulkCommandValidator.cs
namespace DoubleStar.Modules.Inventory.Application.Commands.RestockBulkCommand;

public sealed class RestockBulkCommandValidator : AbstractValidator<RestockBulkCommand>
{
    public RestockBulkCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reference).MaximumLength(200);
    }
}