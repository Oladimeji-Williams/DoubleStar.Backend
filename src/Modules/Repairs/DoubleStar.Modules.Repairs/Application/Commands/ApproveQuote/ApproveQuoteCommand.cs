// Application/Commands/ApproveQuoteCommand/ApproveQuoteCommand.cs
namespace DoubleStar.Modules.Repairs.Application.Commands.ApproveQuoteCommand;

public sealed record ApproveQuoteCommand(int TicketId) : IRequest<Result>;