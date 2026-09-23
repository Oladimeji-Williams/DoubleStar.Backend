// Api/Contracts/RecordDiagnosisRequest.cs
namespace DoubleStar.Modules.Repairs.Api.Contracts;

public sealed record RecordDiagnosisRequest(string DiagnosisNotes, long QuotedPriceKobo);