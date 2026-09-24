// .../GetRepairTurnaroundQueryValidator.cs
namespace DoubleStar.Modules.Reporting.Application.Queries.GetRepairTurnaroundQuery;

public sealed class GetRepairTurnaroundQueryValidator : AbstractValidator<GetRepairTurnaroundQuery>
{
    public GetRepairTurnaroundQueryValidator() =>
        RuleFor(x => x.ToUtc).GreaterThanOrEqualTo(x => x.FromUtc).WithMessage("'to' must not be before 'from'.");
}