// .../GetAllSalesQueryHandler.cs
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Application.Mappings;

namespace DoubleStar.Modules.Sales.Application.Queries.GetAllSalesQuery;

public sealed class GetAllSalesQueryHandler(ISaleRepository saleRepository)
    : IRequestHandler<GetAllSalesQuery, Result<IReadOnlyList<SaleDto>>>
{
    public async Task<Result<IReadOnlyList<SaleDto>>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
    {
        var sales = await saleRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<SaleDto>>.Success(sales.Select(s => s.ToDto()).ToList());
    }
}