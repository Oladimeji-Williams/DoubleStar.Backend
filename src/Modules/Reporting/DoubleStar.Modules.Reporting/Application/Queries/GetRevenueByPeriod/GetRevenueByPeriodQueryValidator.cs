// .../GetRevenueByPeriodQueryValidator.cs
namespace DoubleStar.Modules.Reporting.Application.Queries.GetRevenueByPeriodQuery;

public sealed class GetRevenueByPeriodQueryValidator : AbstractValidator<GetRevenueByPeriodQuery>
{
    public GetRevenueByPeriodQueryValidator() =>
        RuleFor(x => x.ToUtc).GreaterThanOrEqualTo(x => x.FromUtc).WithMessage("'to' must not be before 'from'.");
}