// Application/Queries/GetAllSalesQuery/GetAllSalesQueryHandler.cs — full replacement
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Application.Mappings;

namespace DoubleStar.Modules.Sales.Application.Queries.GetAllSalesQuery;

public sealed class GetAllSalesQueryHandler(ISaleRepository saleRepository, IProductCatalog productCatalog)
    : IRequestHandler<GetAllSalesQuery, Result<IReadOnlyList<SaleDto>>>
{
    public async Task<Result<IReadOnlyList<SaleDto>>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
    {
        var sales = await saleRepository.GetAllAsync(cancellationToken);
        var dtos = await sales.Select(s => s.ToDto()).ToList().WithProductNamesAsync(productCatalog, cancellationToken);
        return Result<IReadOnlyList<SaleDto>>.Success(dtos);
    }
}