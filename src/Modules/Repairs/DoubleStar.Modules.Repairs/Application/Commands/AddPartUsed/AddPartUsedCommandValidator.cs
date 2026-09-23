// .../AddPartUsedCommandValidator.cs
namespace DoubleStar.Modules.Repairs.Application.Commands.AddPartUsedCommand;

public sealed class AddPartUsedCommandValidator : AbstractValidator<AddPartUsedCommand>
{
    public AddPartUsedCommandValidator() => RuleFor(x => x.Quantity).GreaterThan(0);
}