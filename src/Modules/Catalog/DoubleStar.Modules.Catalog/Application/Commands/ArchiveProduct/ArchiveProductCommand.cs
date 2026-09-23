// Application/Commands/ArchiveProductCommand/ArchiveProductCommand.cs
namespace DoubleStar.Modules.Catalog.Application.Commands.ArchiveProductCommand;

public sealed record ArchiveProductCommand(int Id) : IRequest<Result>;