// Application/Commands/AddSerializedUnitCommand/AddSerializedUnitCommand.cs
namespace DoubleStar.Modules.Inventory.Application.Commands.AddSerializedUnitCommand;

public sealed record AddSerializedUnitCommand(int ProductId, string SerialNumber) : IRequest<Result>;