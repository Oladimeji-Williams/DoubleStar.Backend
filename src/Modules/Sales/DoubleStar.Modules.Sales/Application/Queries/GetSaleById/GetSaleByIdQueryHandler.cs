// Application/Queries/GetSaleByIdQuery/GetSaleByIdQueryHandler.cs — full replacement
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Application.Errors;
using DoubleStar.Modules.Sales.Application.Mappings;

namespace DoubleStar.Modules.Sales.Application.Queries.GetSaleByIdQuery;

public sealed class GetSaleByIdQueryHandler(ISaleRepository saleRepository, IProductCatalog productCatalog)
    : IRequestHandler<GetSaleByIdQuery, Result<SaleDto>>
{
    public async Task<Result<SaleDto>> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
        {
            return Result<SaleDto>.Failure(SalesErrors.NotFound(request.Id));
        }

        var dto = await sale.ToDto().WithProductNamesAsync(productCatalog, cancellationToken);
        return Result<SaleDto>.Success(dto);
    }
}