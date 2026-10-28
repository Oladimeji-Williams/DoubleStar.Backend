// The ONLY place Orders talks to Inventory. Reserve at checkout, release on cancel/expiry, consume on payment.
using Microsoft.Extensions.Logging;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Application.Services;

public sealed record StockRequest(int ProductId, string ProductName, int Quantity, bool IsSerialized);

public sealed record ReservedStock(int ProductId, int Quantity, bool IsSerialized, IReadOnlyList<string> SerialNumbers);

public sealed class OrderStockService(IStockAdjuster stockAdjuster, ILogger<OrderStockService> logger)
{
    /// <summary>All-or-nothing: if any line can't be reserved, everything reserved so far is released.</summary>
    public async Task<Result<IReadOnlyList<ReservedStock>>> ReserveAsync(
        IReadOnlyList<StockRequest> requests, CancellationToken cancellationToken)
    {
        var reserved = new List<ReservedStock>();

        foreach (var request in requests)
        {
            if (request.IsSerialized)
            {
                var result = await stockAdjuster.ReserveSerializedAsync(request.ProductId, request.Quantity, cancellationToken);
                if (result.IsFailure)
                {
                    await ReleaseAsync(reserved, cancellationToken);
                    return Result<IReadOnlyList<ReservedStock>>.Failure(OrderErrors.OutOfStock(request.ProductName));
                }

                reserved.Add(new ReservedStock(request.ProductId, request.Quantity, true, result.Value!));
            }
            else
            {
                var result = await stockAdjuster.ReserveAsync(request.ProductId, request.Quantity, cancellationToken);
                if (result.IsFailure)
                {
                    await ReleaseAsync(reserved, cancellationToken);
                    return Result<IReadOnlyList<ReservedStock>>.Failure(OrderErrors.OutOfStock(request.ProductName));
                }

                reserved.Add(new ReservedStock(request.ProductId, request.Quantity, false, []));
            }
        }

        return Result<IReadOnlyList<ReservedStock>>.Success(reserved);
    }

    public async Task ReleaseAsync(IEnumerable<ReservedStock> reserved, CancellationToken cancellationToken)
    {
        foreach (var stock in reserved)
        {
            await ReleaseOneAsync(stock.ProductId, stock.Quantity, stock.IsSerialized, stock.SerialNumbers, cancellationToken);
        }
    }

    /// <summary>Releases whatever an order still holds. Safe to call twice.</summary>
    public async Task ReleaseAsync(Order order, CancellationToken cancellationToken)
    {
        foreach (var line in order.Lines.Where(l => !l.IsStockConsumed))
        {
            await ReleaseOneAsync(line.ProductId, line.Quantity, line.IsSerialized, line.SerialNumbers, cancellationToken);
        }
    }

    /// <summary>Converts reservations into real deductions. Lines already consumed are skipped (idempotent).</summary>
    public async Task<Result> ConsumeAsync(Order order, CancellationToken cancellationToken)
    {
        var failures = new List<Error>();

        foreach (var line in order.Lines.Where(l => !l.IsStockConsumed))
        {
            var result = line.IsSerialized
                ? await stockAdjuster.ConsumeSerializedReservationAsync(line.SerialNumbers, cancellationToken)
                : await stockAdjuster.ConsumeReservationAsync(line.ProductId, line.Quantity, cancellationToken);

            if (result.IsFailure)
            {
                failures.AddRange(result.Errors);
                continue;
            }

            order.MarkLineStockConsumed(line.Id);
        }

        return failures.Count == 0 ? Result.Success() : Result.Failure(failures);
    }

    private async Task ReleaseOneAsync(
        int productId, int quantity, bool isSerialized, IReadOnlyCollection<string> serials, CancellationToken cancellationToken)
    {
        var result = isSerialized
            ? await stockAdjuster.ReleaseSerializedReservationAsync(serials, cancellationToken)
            : await stockAdjuster.ReleaseReservationAsync(productId, quantity, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogError(
                "Could not release reserved stock for product {ProductId}: {Errors}",
                productId, string.Join("; ", result.Errors.Select(e => e.Code)));
        }
    }
}