namespace DoubleStar.Modules.Repairs.Application.Commands.RecordDiagnosisCommand;

public sealed record RecordDiagnosisCommand(int TicketId, string DiagnosisNotes, long QuotedPriceKobo) : IRequest<Result>;