// .../VoidSaleCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.Errors;

namespace DoubleStar.Modules.Sales.Application.Commands.VoidSaleCommand;

public sealed class VoidSaleCommandHandler(ISaleRepository saleRepository, IStockAdjuster stockAdjuster)
    : IRequestHandler<VoidSaleCommand, Result>
{
    public async Task<Result> Handle(VoidSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
        {
            return Result.Failure(SalesErrors.NotFound(request.SaleId));
        }

        try
        {
            sale.Void();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Sales.InvalidState", ex.Message, ErrorType.Conflict));
        }

        // Only bulk lines that were reserved (and never deducted) need releasing —
        // serialized lines were only checked, never reserved, so there's nothing to undo for them.
        foreach (var line in sale.Lines.Where(l => l.SerialNumber is null && !l.IsStockDeducted))
        {
            await stockAdjuster.ReleaseReservationAsync(line.ProductId, line.Quantity, cancellationToken);
        }

        await saleRepository.UpdateAsync(sale, cancellationToken);
        return Result.Success();
    }
}