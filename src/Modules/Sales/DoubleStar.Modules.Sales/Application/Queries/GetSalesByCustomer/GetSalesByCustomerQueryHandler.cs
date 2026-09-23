// .../GetSalesByCustomerQueryHandler.cs
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Application.Mappings;

namespace DoubleStar.Modules.Sales.Application.Queries.GetSalesByCustomerQuery;

public sealed class GetSalesByCustomerQueryHandler(ISaleRepository saleRepository)
    : IRequestHandler<GetSalesByCustomerQuery, Result<IReadOnlyList<SaleDto>>>
{
    public async Task<Result<IReadOnlyList<SaleDto>>> Handle(
        GetSalesByCustomerQuery request, CancellationToken cancellationToken)
    {
        var sales = await saleRepository.GetAllForCustomerAsync(request.CustomerId, cancellationToken);
        return Result<IReadOnlyList<SaleDto>>.Success(sales.Select(s => s.ToDto()).ToList());
    }
}