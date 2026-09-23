// Application/Commands/RestockBulkCommand/RestockBulkCommand.cs
namespace DoubleStar.Modules.Inventory.Application.Commands.RestockBulkCommand;

public sealed record RestockBulkCommand(int ProductId, int Quantity, string? Reference) : IRequest<Result>;