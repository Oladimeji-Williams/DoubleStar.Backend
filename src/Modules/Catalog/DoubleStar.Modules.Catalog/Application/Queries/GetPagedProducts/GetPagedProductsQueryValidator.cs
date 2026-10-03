// .../GetPagedProductsQueryValidator.cs
namespace DoubleStar.Modules.Catalog.Application.Queries.GetPagedProductsQuery;

public sealed class GetPagedProductsQueryValidator : AbstractValidator<GetPagedProductsQuery>
{
    public GetPagedProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}