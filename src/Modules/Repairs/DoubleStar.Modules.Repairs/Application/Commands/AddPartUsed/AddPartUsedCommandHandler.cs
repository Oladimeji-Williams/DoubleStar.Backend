// .../AddPartUsedCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Errors;
using DoubleStar.Modules.Repairs.Application.Mappings;

namespace DoubleStar.Modules.Repairs.Application.Commands.AddPartUsedCommand;

public sealed class AddPartUsedCommandHandler(
    IRepairTicketRepository repairTicketRepository, IProductCatalog productCatalog, IStockAdjuster stockAdjuster)
    : IRequestHandler<AddPartUsedCommand, Result<RepairPartDto>>
{
    public async Task<Result<RepairPartDto>> Handle(AddPartUsedCommand request, CancellationToken cancellationToken)
    {
        var ticket = await repairTicketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result<RepairPartDto>.Failure(RepairErrors.NotFound(request.TicketId));
        }

        var product = await productCatalog.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<RepairPartDto>.Failure(RepairErrors.ProductNotFound(request.ProductId));
        }

        // Deducted immediately, not reserved — see the module-level note on why.
        var deductResult = await stockAdjuster.DeductAsync(request.ProductId, request.Quantity, cancellationToken);
        if (deductResult.IsFailure)
        {
            return Result<RepairPartDto>.Failure(deductResult.Errors);
        }

        try
        {
            var part = ticket.AddPart(request.ProductId, request.Quantity, product.UnitPriceKobo);
            await repairTicketRepository.UpdateAsync(ticket, cancellationToken);
            return Result<RepairPartDto>.Success(part.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return Result<RepairPartDto>.Failure(RepairErrors.InvalidTransition(ex.Message));
        }
    }
}