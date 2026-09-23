// .../AddSaleLineCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Application.Errors;
using DoubleStar.Modules.Sales.Application.Mappings;

namespace DoubleStar.Modules.Sales.Application.Commands.AddSaleLineCommand;

public sealed class AddSaleLineCommandHandler(
    ISaleRepository saleRepository, IProductCatalog productCatalog,
    IStockAdjuster stockAdjuster, IStockLevelReader stockLevelReader)
    : IRequestHandler<AddSaleLineCommand, Result<SaleLineDto>>
{
    public async Task<Result<SaleLineDto>> Handle(AddSaleLineCommand request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
        {
            return Result<SaleLineDto>.Failure(SalesErrors.NotFound(request.SaleId));
        }

        var product = await productCatalog.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<SaleLineDto>.Failure(SalesErrors.ProductNotFound(request.ProductId));
        }

        int quantity;

        if (product.TrackingMode == StockTrackingMode.Serialized)
        {
            if (string.IsNullOrWhiteSpace(request.SerialNumber))
            {
                return Result<SaleLineDto>.Failure(SalesErrors.SerialRequired());
            }

            // Checked here for a fast, friendly error at add-line time; re-checked
            // authoritatively by Inventory again at CompleteSaleCommand time.
            var unit = await stockLevelReader.GetBySerialAsync(request.SerialNumber, cancellationToken);
            if (unit is null || unit.Status != "InStock")
            {
                return Result<SaleLineDto>.Failure(SalesErrors.SerialNotAvailable(request.SerialNumber));
            }

            quantity = 1;
        }
        else
        {
            if (request.SerialNumber is not null)
            {
                return Result<SaleLineDto>.Failure(SalesErrors.QuantityNotAllowed());
            }

            quantity = request.Quantity ?? 1;

            var reserveResult = await stockAdjuster.ReserveAsync(request.ProductId, quantity, cancellationToken);
            if (reserveResult.IsFailure)
            {
                return Result<SaleLineDto>.Failure(reserveResult.Errors);
            }
        }

        try
        {
            var line = sale.AddLine(request.ProductId, product.UnitPriceKobo, quantity, request.SerialNumber);
            await saleRepository.UpdateAsync(sale, cancellationToken);
            return Result<SaleLineDto>.Success(line.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return Result<SaleLineDto>.Failure(new Error("Sales.InvalidState", ex.Message, ErrorType.Conflict));
        }
    }
}