using FluentValidation;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetPagedProducts;

public sealed class GetPagedProductsQueryValidator
    : AbstractValidator<GetPagedProductsQuery>
{
    public GetPagedProductsQueryValidator()
    {
        RuleFor(x => x.Query.Search)
            .MaximumLength(200);
    }
}