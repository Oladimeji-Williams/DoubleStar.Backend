using FluentValidation;

namespace DoubleStar.Modules.Customers.Application.Queries.SearchCustomersQuery;

public sealed class SearchCustomersQueryValidator
    : AbstractValidator<SearchCustomersQuery>
{
    public SearchCustomersQueryValidator()
    {
        RuleFor(x => x.Query.Search)
            .MaximumLength(200);
    }
}