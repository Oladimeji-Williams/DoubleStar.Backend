// Api/Contracts/CreateSaleRequest.cs
namespace DoubleStar.Modules.Sales.Api.Contracts;

public sealed record CreateSaleRequest(Guid? CustomerId, string? WalkInName, string? WalkInPhone);