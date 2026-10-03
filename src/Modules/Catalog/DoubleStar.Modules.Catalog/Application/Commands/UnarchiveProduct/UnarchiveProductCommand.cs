// Catalog/Application/Commands/UnarchiveProductCommand/UnarchiveProductCommand.cs
namespace DoubleStar.Modules.Catalog.Application.Commands.UnarchiveProductCommand;

public sealed record UnarchiveProductCommand(int Id) : IRequest<Result>;