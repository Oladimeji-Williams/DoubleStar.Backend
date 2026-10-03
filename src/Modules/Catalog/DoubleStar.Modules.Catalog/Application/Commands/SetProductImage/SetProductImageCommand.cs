// Modules/Catalog/Application/Commands/SetProductImageCommand/SetProductImageCommand.cs
using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Commands.SetProductImageCommand;

public sealed record SetProductImageCommand(int ProductId, string? ImageUrl) : IRequest<Result<ProductDto>>;