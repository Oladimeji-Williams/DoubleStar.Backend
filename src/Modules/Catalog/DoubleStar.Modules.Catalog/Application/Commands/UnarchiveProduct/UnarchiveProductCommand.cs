namespace DoubleStar.Modules.Catalog.Application.Commands.UnarchiveProductCommand;

public sealed record UnarchiveProductCommand(int Id) : IRequest<Result>;