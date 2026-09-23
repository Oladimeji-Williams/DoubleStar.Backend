// .../CompleteSaleCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Application.Errors;
using DoubleStar.Modules.Sales.Application.Mappings;
using DoubleStar.Modules.Sales.Domain.Enums;

namespace DoubleStar.Modules.Sales.Application.Commands.CompleteSaleCommand;

public sealed class CompleteSaleCommandHandler(ISaleRepository saleRepository, IStockAdjuster stockAdjuster)
    : IRequestHandler<CompleteSaleCommand, Result<SaleDto>>
{
    public async Task<Result<SaleDto>> Handle(CompleteSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
        {
            return Result<SaleDto>.Failure(SalesErrors.NotFound(request.SaleId));
        }
        if (sale.Status != SaleStatus.Draft)
        {
            return Result<SaleDto>.Failure(SalesErrors.NotDraft(sale.Id));
        }
        if (sale.Lines.Count == 0)
        {
            return Result<SaleDto>.Failure(SalesErrors.NoLines(sale.Id));
        }

        foreach (var line in sale.Lines.Where(l => !l.IsStockDeducted))
        {
            var deductResult = line.SerialNumber is not null
                ? await stockAdjuster.DeductSerializedAsync(line.SerialNumber, cancellationToken)
                : await stockAdjuster.DeductAsync(line.ProductId, line.Quantity, cancellationToken);

            if (deductResult.IsFailure)
            {
                // Save progress on the lines that DID succeed — a retry of this
                // command will pick up only from here, not double-deduct.
                await saleRepository.UpdateAsync(sale, cancellationToken);
                return Result<SaleDto>.Failure(deductResult.Errors);
            }

            sale.MarkLineDeducted(line.Id);
        }

        sale.Complete();
        await saleRepository.UpdateAsync(sale, cancellationToken);

        return Result<SaleDto>.Success(sale.ToDto());
    }
}