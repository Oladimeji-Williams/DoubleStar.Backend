// Api/Contracts/RestockBulkRequest.cs
namespace DoubleStar.Modules.Inventory.Api.Contracts;

public sealed record RestockBulkRequest(int ProductId, int Quantity, string? Reference);