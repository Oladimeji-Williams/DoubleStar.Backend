// .../GetSerializedUnitQueryHandler.cs
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Application.Errors;

namespace DoubleStar.Modules.Inventory.Application.Queries.GetSerializedUnitQuery;

public sealed class GetSerializedUnitQueryHandler(ISerializedUnitRepository serializedUnitRepository)
    : IRequestHandler<GetSerializedUnitQuery, Result<SerializedUnitDto>>
{
    public async Task<Result<SerializedUnitDto>> Handle(GetSerializedUnitQuery request, CancellationToken cancellationToken)
    {
        var unit = await serializedUnitRepository.GetBySerialAsync(request.SerialNumber, cancellationToken);
        if (unit is null)
        {
            return Result<SerializedUnitDto>.Failure(InventoryErrors.SerialNotFound(request.SerialNumber));
        }

        return Result<SerializedUnitDto>.Success(
            new SerializedUnitDto(unit.Id, unit.ProductId, unit.SerialNumber, unit.Status.ToString()));
    }
}