using DoubleStar.SharedKernel.Domain;
using DoubleStar.Modules.Inventory.Domain.Enums;

namespace DoubleStar.Modules.Inventory.Domain.Entities;

/// <summary>One physical unit of a Serialized product — tracked by IMEI/serial.</summary>
public sealed class SerializedUnit : GuidEntity
{
    public int ProductId { get; private set; }
    public string SerialNumber { get; private set; } = null!;
    public SerializedUnitStatus Status { get; private set; }

    private SerializedUnit() { }

    public static SerializedUnit Receive(int productId, string serialNumber) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = productId,
        SerialNumber = serialNumber.Trim().ToUpperInvariant(),
        Status = SerializedUnitStatus.InStock,
    };

    public bool TryMarkSold()
    {
        if (Status == SerializedUnitStatus.Sold) return false;
        Status = SerializedUnitStatus.Sold;
        return true;
    }

    public bool TryReserve()
    {
        if (Status != SerializedUnitStatus.InStock)
            return false;

        Status = SerializedUnitStatus.Reserved;
        return true;
    }

    public bool ReleaseReservation()
    {
        if (Status != SerializedUnitStatus.Reserved)
            return false;

        Status = SerializedUnitStatus.InStock;
        return true;
    }

    public bool TryMarkReservedUnitSold()
    {
        if (Status != SerializedUnitStatus.Reserved)
            return false;

        Status = SerializedUnitStatus.Sold;
        return true;
    }
}