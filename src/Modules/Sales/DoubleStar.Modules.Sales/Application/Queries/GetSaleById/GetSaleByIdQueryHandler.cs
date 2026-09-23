// .../GetSaleByIdQueryHandler.cs
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Application.Errors;
using DoubleStar.Modules.Sales.Application.Mappings;

namespace DoubleStar.Modules.Sales.Application.Queries.GetSaleByIdQuery;

public sealed class GetSaleByIdQueryHandler(ISaleRepository saleRepository)
    : IRequestHandler<GetSaleByIdQuery, Result<SaleDto>>
{
    public async Task<Result<SaleDto>> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(request.Id, cancellationToken);
        return sale is null
            ? Result<SaleDto>.Failure(SalesErrors.NotFound(request.Id))
            : Result<SaleDto>.Success(sale.ToDto());
    }
}