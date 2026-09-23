// Application/Commands/UpdateProductCommand/UpdateProductCommand.cs
namespace DoubleStar.Modules.Catalog.Application.Commands.UpdateProductCommand;

public sealed record UpdateProductCommand(int Id, string Name, string? Description, int? CategoryId, int? BrandId)
    : IRequest<Result>;