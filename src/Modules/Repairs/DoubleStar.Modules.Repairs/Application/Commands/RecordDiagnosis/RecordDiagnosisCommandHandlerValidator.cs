// .../RecordDiagnosisCommandValidator.cs
namespace DoubleStar.Modules.Repairs.Application.Commands.RecordDiagnosisCommand;

public sealed class RecordDiagnosisCommandValidator : AbstractValidator<RecordDiagnosisCommand>
{
    public RecordDiagnosisCommandValidator()
    {
        RuleFor(x => x.DiagnosisNotes).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.QuotedPriceKobo).GreaterThanOrEqualTo(0);
    }
}