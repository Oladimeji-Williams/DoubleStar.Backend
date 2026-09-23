// Api/Contracts/ApiResponse.cs
namespace DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;

public sealed record ApiResponse<T>(bool Success, T? Data, IReadOnlyList<ApiError>? Errors);