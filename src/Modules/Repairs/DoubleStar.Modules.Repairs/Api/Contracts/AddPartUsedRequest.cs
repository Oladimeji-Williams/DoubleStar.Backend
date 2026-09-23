// Api/Contracts/AddPartUsedRequest.cs
namespace DoubleStar.Modules.Repairs.Api.Contracts;

public sealed record AddPartUsedRequest(int ProductId, int Quantity);