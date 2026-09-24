// .../GetSalesSummaryQueryValidator.cs
namespace DoubleStar.Modules.Reporting.Application.Queries.GetSalesSummaryQuery;

public sealed class GetSalesSummaryQueryValidator : AbstractValidator<GetSalesSummaryQuery>
{
    public GetSalesSummaryQueryValidator() =>
        RuleFor(x => x.ToUtc).GreaterThanOrEqualTo(x => x.FromUtc).WithMessage("'to' must not be before 'from'.");
}