// Api/Contracts/ApiError.cs
namespace DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;

public sealed record ApiError(string Code, string Message, string Type);