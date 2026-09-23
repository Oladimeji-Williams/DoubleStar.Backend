// .../AddSerializedUnitCommandHandler.cs
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Application.Errors;
using DoubleStar.Modules.Inventory.Domain.Entities;
using DoubleStar.Modules.Inventory.Domain.Enums;

namespace DoubleStar.Modules.Inventory.Application.Commands.AddSerializedUnitCommand;

public sealed class AddSerializedUnitCommandHandler(
    ISerializedUnitRepository serializedUnitRepository, IStockMovementRepository stockMovementRepository)
    : IRequestHandler<AddSerializedUnitCommand, Result>
{
    public async Task<Result> Handle(AddSerializedUnitCommand request, CancellationToken cancellationToken)
    {
        if (await serializedUnitRepository.GetBySerialAsync(request.SerialNumber, cancellationToken) is not null)
        {
            return Result.Failure(InventoryErrors.SerialAlreadyExists(request.SerialNumber));
        }

        var unit = SerializedUnit.Receive(request.ProductId, request.SerialNumber);
        await serializedUnitRepository.AddAsync(unit, cancellationToken);

        await stockMovementRepository.AddAsync(
            StockMovement.Create(request.ProductId, StockMovementType.In, 1, request.SerialNumber),
            cancellationToken);

        return Result.Success();
    }
}