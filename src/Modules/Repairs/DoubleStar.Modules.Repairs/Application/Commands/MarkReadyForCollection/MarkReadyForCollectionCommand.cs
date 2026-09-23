// Application/Commands/MarkReadyForCollectionCommand/MarkReadyForCollectionCommand.cs
namespace DoubleStar.Modules.Repairs.Application.Commands.MarkReadyForCollectionCommand;

public sealed record MarkReadyForCollectionCommand(int TicketId) : IRequest<Result>;