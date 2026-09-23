// .../AddSerializedUnitCommandValidator.cs
namespace DoubleStar.Modules.Inventory.Application.Commands.AddSerializedUnitCommand;

public sealed class AddSerializedUnitCommandValidator : AbstractValidator<AddSerializedUnitCommand>
{
    public AddSerializedUnitCommandValidator() => RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(100);
}