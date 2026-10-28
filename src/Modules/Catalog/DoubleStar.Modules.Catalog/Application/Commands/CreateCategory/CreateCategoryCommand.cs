namespace DoubleStar.Modules.Catalog.Application.Commands.CreateCategoryCommand;

public sealed record CreateCategoryCommand(string Name) : IRequest<Result<int>>;