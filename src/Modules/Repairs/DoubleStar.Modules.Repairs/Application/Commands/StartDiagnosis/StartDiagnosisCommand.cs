// Application/Commands/StartDiagnosisCommand/StartDiagnosisCommand.cs
namespace DoubleStar.Modules.Repairs.Application.Commands.StartDiagnosisCommand;
public sealed record StartDiagnosisCommand(int TicketId) : IRequest<Result>;