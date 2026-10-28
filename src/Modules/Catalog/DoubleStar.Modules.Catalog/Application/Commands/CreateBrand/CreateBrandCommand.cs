namespace DoubleStar.Modules.Catalog.Application.Commands.CreateBrandCommand;

public sealed record CreateBrandCommand(string Name) : IRequest<Result<int>>;