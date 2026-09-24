// .../GetInventoryValuationQueryValidator.cs
namespace DoubleStar.Modules.Reporting.Application.Queries.GetInventoryValuationQuery;

public sealed class GetInventoryValuationQueryValidator : AbstractValidator<GetInventoryValuationQuery>
{
    public GetInventoryValuationQueryValidator() => RuleFor(x => x.LowStockThreshold).GreaterThanOrEqualTo(0);
}