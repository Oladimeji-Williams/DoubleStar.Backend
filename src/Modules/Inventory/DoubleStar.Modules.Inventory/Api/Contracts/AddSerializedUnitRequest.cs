// Api/Contracts/AddSerializedUnitRequest.cs
namespace DoubleStar.Modules.Inventory.Api.Contracts;

public sealed record AddSerializedUnitRequest(int ProductId, string SerialNumber);